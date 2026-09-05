using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.TexasHoldem
{
    public sealed class TexasHoldemPlayerCards
    {
        public TexasHoldemPlayerCards(Card firstCard, Card secondCard)
        {
            FirstCard = firstCard;
            SecondCard = secondCard;
        }

        public Card FirstCard { get; }

        public Card SecondCard { get; }

        public IReadOnlyList<Card> Cards => new[] { FirstCard, SecondCard };

        public Card this[int index] => index switch
        {
            0 => FirstCard,
            1 => SecondCard,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}