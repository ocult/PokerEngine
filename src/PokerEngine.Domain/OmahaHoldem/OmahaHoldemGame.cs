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

        private PokerHand GetBestHandForPlayer(ushort player)
        {
            OmahaHoldemPlayerCards playerCards = _playersCards[player];
            if (_communityCards.Count < 3)
            {
                throw new InvalidOperationException("The community cards must be flop enough before evaluating best hands.");
            }

            List<Card[]> playerCombos = GetCombinations(playerCards.Cards, 2);
            List<Card[]> communityCombos = GetCombinations(_communityCards, 3);

            List<PokerHand> possibleHands = new();
            foreach (Card[] pCombo in playerCombos)
            {
                foreach (Card[] cCombo in communityCombos)
                {
                    possibleHands.Add(new PokerHand(pCombo[0], pCombo[1], cCombo[0], cCombo[1], cCombo[2]));
                }
            }

            return possibleHands.OrderBy(h => h).First();
        }

        private static List<Card[]> GetCombinations(IReadOnlyList<Card> cards, int k)
        {
            List<Card[]> result = new();
            GetCombinationsHelper(cards, k, 0, new Card[k], 0, result);
            return result;
        }

        private static void GetCombinationsHelper(IReadOnlyList<Card> cards, int k, int start, Card[] current, int index, List<Card[]> result)
        {
            if (index == k)
            {
                Card[] combination = new Card[k];
                Array.Copy(current, combination, k);
                result.Add(combination);
                return;
            }

            for (int i = start; i <= cards.Count - k + index; i++)
            {
                current[index] = cards[i];
                GetCombinationsHelper(cards, k, i + 1, current, index + 1, result);
            }
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