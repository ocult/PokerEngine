using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.OmahaHoldem
{
    public sealed class OmahaHoldemPlayerCards : HoldemPlayerCards
    {
        public OmahaHoldemPlayerCards(Card firstCard, Card secondCard, Card thirdCard, Card fourthCard)
            : base(new[] { firstCard, secondCard, thirdCard, fourthCard })
        {
        }

        public OmahaHoldemPlayerCards(IReadOnlyList<Card> cards)
            : base(cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            if (cards.Count < 4) throw new ArgumentException("Omaha Holdem player cards must contain at least 4 cards.", nameof(cards));
        }

        public Card FirstCard => Cards[0];

        public Card SecondCard => Cards[1];

        public Card ThirdCard => Cards[2];

        public Card FourthCard => Cards[3];
    }
}