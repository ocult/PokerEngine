using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.TexasHoldem
{
    public sealed class TexasHoldemGame : HoldemGame<TexasHoldemPlayerCards>
    {
        
        override protected int CardsPerPlayer => 2;

        public TexasHoldemGame(ushort players, CardDeck? deck = null)
            : base(players, cards => new TexasHoldemPlayerCards(cards), deck)
        {
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
    }
}
