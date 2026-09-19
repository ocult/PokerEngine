using PokerEngine.Domain.Models;

namespace PokerEngine.Domain.OmahaHoldem
{
    public sealed class OmahaHoldemPlayerCards
    {
        public OmahaHoldemPlayerCards(Card firstCard, Card secondCard, Card thirdCard, Card fourthCard)
        {
            FirstCard = firstCard;
            SecondCard = secondCard;
            ThirdCard = thirdCard;
            FourthCard = fourthCard;
        }

        public Card FirstCard { get; }

        public Card SecondCard { get; }

        public Card ThirdCard { get; }

        public Card FourthCard { get; }

        public IReadOnlyList<Card> Cards => new[] { FirstCard, SecondCard, ThirdCard, FourthCard };

        public Card this[int index] => index switch
        {
            0 => FirstCard,
            1 => SecondCard,
            2 => ThirdCard,
            3 => FourthCard,
            _ => throw new ArgumentOutOfRangeException(nameof(index))
        };
    }
}