namespace PokerEngine.Domain.Models
{
    public abstract class HoldemPlayerCards
    {
        protected HoldemPlayerCards(IReadOnlyList<Card> cards)
        {
            Cards = cards ?? throw new ArgumentNullException(nameof(cards));
        }

        public IReadOnlyList<Card> Cards { get; }

        public Card this[int index]
        {
            get
            {
                if (index < 0 || index >= Cards.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }
                return Cards[index];
            }
        }
    }
}
