using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.OmahaHoldem
{
    public sealed class OmahaHoldemGame : HoldemGame<OmahaHoldemPlayerCards>
    {

        override protected int CardsPerPlayer => 4;

        public OmahaHoldemGame(ushort players, CardDeck? deck = null)
            : base(players, cards => new OmahaHoldemPlayerCards(cards), deck)
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
    }
}