using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.TexasHoldem
{
    public sealed class TexasHoldemGame : PokerGame
    {
        private const int MaxPlayersPerDeck = 21;

        private readonly CardDeck _deck;
        private readonly Dictionary<ushort, TexasHoldemPlayerCards> _playersCards;
        private readonly List<Card> _communityCards;

        public TexasHoldemGame(ushort players, CardDeck? deck = null)
        {
            if (players == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(players));
            }

            if (players > MaxPlayersPerDeck)
            {
                throw new InvalidOperationException(
                    $"A standard deck cannot support {players} players in Texas Hold'em. The maximum supported is {MaxPlayersPerDeck}.");
            }

            Players = players;
            _deck = deck ?? new CardDeck();
            Dictionary<ushort, List<Card>> playerCards = new Dictionary<ushort, List<Card>>();
            _playersCards = new Dictionary<ushort, TexasHoldemPlayerCards>();
            _communityCards = new List<Card>();

            for (ushort i = 1; i <= players; i++)
            {
                playerCards[i] = new List<Card>();
            }

            for (ushort round = 0; round < 2; round++)
            {
                for (ushort i = 1; i <= players; i++)
                {
                    playerCards[i].Add(_deck.Pick());
                }
            }

            foreach (KeyValuePair<ushort, List<Card>> player in playerCards)
            {
                _playersCards[player.Key] = new TexasHoldemPlayerCards(player.Value[0], player.Value[1]);
            }

            Stage = TexasHoldemStage.PreFlop;
        }

        public override ushort Players { get; }

        public TexasHoldemStage Stage { get; private set; }

        public IReadOnlyDictionary<ushort, TexasHoldemPlayerCards> PlayersCards => _playersCards;

        public IReadOnlyList<Card> CommunityCards => _communityCards.AsReadOnly();

        public IReadOnlyList<Card> Continue()
        {
            switch (Stage)
            {
                case TexasHoldemStage.PreFlop:
                    BurnTwoCards();
                    DealCommunityCards(3);
                    Stage = TexasHoldemStage.Flop;
                    return CommunityCards;
                case TexasHoldemStage.Flop:
                    BurnOneCard();
                    DealCommunityCards(1);
                    Stage = TexasHoldemStage.Turn;
                    return CommunityCards;
                case TexasHoldemStage.Turn:
                    BurnOneCard();
                    DealCommunityCards(1);
                    Stage = TexasHoldemStage.River;
                    return CommunityCards;
                case TexasHoldemStage.River:
                    Stage = TexasHoldemStage.Complete;
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

            if (_communityCards.Count == 5)
            {                
                PokerHand tableHand = new(_communityCards.ToArray());
                hands.Add(0, tableHand);
            }
            
            return hands;
        }

        private PokerHand GetBestHandForPlayer(ushort player)
        {
            TexasHoldemPlayerCards playerCards = _playersCards[player];
            if (_communityCards.Count < 3)
            {
                throw new InvalidOperationException("The community cards must be flop enough before evaluating best hands.");
            }

            List<Card> allCards = new(_communityCards);
            allCards.AddRange(playerCards.Cards);

            List<Card[]> combinations = GetCombinations(allCards, 5);
            List<PokerHand> possibleHands = combinations
                .Where(c => !c.All(card => _communityCards.Contains(card)))
                .Select(c => new PokerHand(c))
                .ToList();

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
