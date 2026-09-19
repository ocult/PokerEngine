using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.OmahaHoldem
{
    public sealed class OmahaHoldemGame : PokerGame
    {
        private const int MaxPlayersPerDeck = 10;

        private readonly CardDeck _deck;
        private readonly Dictionary<ushort, OmahaHoldemPlayerCards> _playersCards;
        private readonly List<Card> _communityCards;

        public OmahaHoldemGame(ushort players, CardDeck? deck = null)
        {
            if (players == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            if (players > MaxPlayersPerDeck)
            {
                throw new InvalidOperationException(
                    $"A standard deck cannot support {players} players in Omaha Hold'em. The maximum supported is {MaxPlayersPerDeck}.");
            }

            Players = players;
            _deck = deck ?? new CardDeck();
            Dictionary<ushort, List<Card>> playerCards = new Dictionary<ushort, List<Card>>();
            _playersCards = new Dictionary<ushort, OmahaHoldemPlayerCards>();
            _communityCards = new List<Card>();

            for (ushort i = 1; i <= players; i++)
            {
                playerCards[i] = new List<Card>();
            }

            for (ushort round = 0; round < 4; round++)
            {
                for (ushort i = 1; i <= players; i++)
                {
                    playerCards[i].Add(_deck.Pick());
                }
            }

            foreach (KeyValuePair<ushort, List<Card>> player in playerCards)
            {
                _playersCards[player.Key] = new OmahaHoldemPlayerCards(player.Value[0], player.Value[1], player.Value[2], player.Value[3]);
            }

            Stage = OmahaHoldemStage.PreFlop;
        }

        public override ushort Players { get; }

        public OmahaHoldemStage Stage { get; private set; }

        public IReadOnlyDictionary<ushort, OmahaHoldemPlayerCards> PlayersCards => _playersCards;

        public IReadOnlyList<Card> CommunityCards => _communityCards.AsReadOnly();

        public IReadOnlyList<Card> Continue()
        {
            switch (Stage)
            {
                case OmahaHoldemStage.PreFlop:
                    BurnTwoCards();
                    DealCommunityCards(3);
                    Stage = OmahaHoldemStage.Flop;
                    return CommunityCards;
                case OmahaHoldemStage.Flop:
                    BurnOneCard();
                    DealCommunityCards(1);
                    Stage = OmahaHoldemStage.Turn;
                    return CommunityCards;
                case OmahaHoldemStage.Turn:
                    BurnOneCard();
                    DealCommunityCards(1);
                    Stage = OmahaHoldemStage.River;
                    return CommunityCards;
                case OmahaHoldemStage.River:
                    Stage = OmahaHoldemStage.Complete;
                    return CommunityCards;
                default:
                    return CommunityCards;
            }
        }

        public IReadOnlyList<KeyValuePair<ushort, PokerHand>> GetBestHands()
        {
            return GetRankedHands();
        }

        protected override IDictionary<ushort, PokerHand> EvaluateBestHands()
        {
            if (_communityCards.Count < 3)
            {
                throw new InvalidOperationException("The community cards must be flop enough before evaluating best hands.");
            }

            Dictionary<ushort, PokerHand> hands = new();

            for (ushort i = 1; i <= Players; i++)
            {
                PokerHand bestHand = GetBestHandForPlayer(i);
                hands.Add(i, bestHand);
            }

            return hands;
        }

        private List<PokerHand> AddCardsToPossibleHands(OmahaHoldemPlayerCards playerCards, Card card1, Card card2, Card card3)
        {
            List<PokerHand> possibleHands =
            [
                new (card1, card2, card3, playerCards[0], playerCards[1]),
                new (card1, card2, card3, playerCards[0], playerCards[2]),
                new (card1, card2, card3, playerCards[0], playerCards[3]),
                new (card1, card2, card3, playerCards[1], playerCards[2]),
                new (card1, card2, card3, playerCards[1], playerCards[3]),
                new (card1, card2, card3, playerCards[2], playerCards[3]),
            ];

            return possibleHands;
        }

        private PokerHand GetBestHandForPlayer(ushort player)
        {
            OmahaHoldemPlayerCards playerCards = _playersCards[player];
            if (_communityCards.Count < 3)
            {
                throw new InvalidOperationException("The community cards must be flop enough before evaluating best hands.");
            }
            List<PokerHand> possibleHands = AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[1], _communityCards[2]);
            if (_communityCards.Count > 3)
            {
                possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[1], _communityCards[3]));
                possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[2], _communityCards[3]));
                possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[1], _communityCards[2], _communityCards[3]));
                if (_communityCards.Count > 4)
                {
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[1], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[2], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[1], _communityCards[2], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[0], _communityCards[3], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[1], _communityCards[3], _communityCards[4]));
                    possibleHands.AddRange(AddCardsToPossibleHands(playerCards, _communityCards[2], _communityCards[3], _communityCards[4]));
                }
            }
            
            return possibleHands.OrderBy(h => h).First();
        }

        private void BurnTwoCards()
        {
            EnsureDeckHasCards(2);
            _deck.Pick();
            _deck.Pick();
        }

        private void BurnOneCard()
        {
            EnsureDeckHasCards(1);
            _deck.Pick();
        }

        private void DealCommunityCards(int quantity)
        {
            EnsureDeckHasCards(quantity);
            for (int i = 0; i < quantity; i++)
            {
                _communityCards.Add(_deck.Pick());
            }
        }

        private void EnsureDeckHasCards(int requiredCards)
        {
            if (_deck.Count < requiredCards)
            {
                throw new InvalidOperationException(
                    $"The deck does not have enough cards to continue the hand. Required: {requiredCards}, available: {_deck.Count}.");
            }
        }
    }
}