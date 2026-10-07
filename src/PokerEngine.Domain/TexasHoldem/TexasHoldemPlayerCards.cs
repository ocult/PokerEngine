using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.TexasHoldem
{
    public sealed class TexasHoldemPlayerCards : HoldemPlayerCards
    {
        public TexasHoldemPlayerCards(Card firstCard, Card secondCard)
            : base(new[] { firstCard, secondCard })
        {
        }

        public TexasHoldemPlayerCards(IReadOnlyList<Card> cards)
            : base(cards)
        {
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            if (cards.Count < 2) throw new ArgumentException("Texas Holdem player cards must contain at least 2 cards.", nameof(cards));
        }

        public Card FirstCard => Cards[0];

        public Card SecondCard => Cards[1];
    }
}