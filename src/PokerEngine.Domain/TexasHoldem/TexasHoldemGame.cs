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
            IReadOnlyList<Card> tableCards = _communityCards;
            List<PokerHand> possibleHands = new();

            for (ushort c = 0; _communityCards.Count > 3 && c < 5; c++)
            {
                Card card1 = c != 0 ? tableCards[0] : playerCards[0];
                Card card2 = c != 1 ? tableCards[1] : playerCards[0];
                Card card3 = c != 2 ? tableCards[2] : playerCards[0];
                Card card4 = c != 3 ? tableCards[3] : playerCards[0];
                Card card5 = default;
                if (_communityCards.Count > 4 || c == 4)
                {
                    card5 = c != 4 ? tableCards[4] : playerCards[0];
                    possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));
                }

                card1 = c != 0 ? tableCards[0] : playerCards[1];
                card2 = c != 1 ? tableCards[1] : playerCards[1];
                card3 = c != 2 ? tableCards[2] : playerCards[1];
                card4 = c != 3 ? tableCards[3] : playerCards[1];
                if (_communityCards.Count > 4 || c == 4)
                {
                    card5 = c != 4 ? tableCards[4] : playerCards[1];
                    possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));
                }
            }

            ushort limit = (ushort)(_communityCards.Count - 2);

            for (ushort c = 0; c < limit; c++)
            {
                Card card1 = playerCards[0];
                Card card2 = playerCards[1];
                Card card3 = tableCards[c];
                Card card4 = tableCards[c + 1];
                Card card5 = tableCards[c + 2];
                possibleHands.Add(new PokerHand(card1, card2, card3, card4, card5));
            }

            return possibleHands.OrderBy(h => h).First();
        }
    }
}