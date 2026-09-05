using PokerEngine.Domain.Models;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class CardDeckTest
    {
        [Fact]
        public void CardDeckTest_New()
        {
            var deck = new CardDeck(false);
            Assert.Equal(52, deck.Count);
            for (ushort c = 2; c < 15; ++c)
            {
                Assert.Equal(new Card(c, SuitEnum.Clubs), deck.Pick());
                Assert.Equal(new Card(c, SuitEnum.Hearts), deck.Pick());
                Assert.Equal(new Card(c, SuitEnum.Spades), deck.Pick());
                Assert.Equal(new Card(c, SuitEnum.Diamonds), deck.Pick());
            }
        }

        [Fact]
        public void CardDeckTest_Order()
        {
            var deck = new CardDeck();
            deck.Order();
            Assert.Equal(52, deck.Count);
            for (ushort s = 1; s < 5; ++s)
            {
                SuitEnum suit = (SuitEnum)s;
                for (ushort c = 2; c < 15; ++c)
                {
                    Assert.Equal(new Card(c, suit), deck.Pick());
                }
            }
        }

        [Fact]
        public void CardDeck_FromCards_PreservesInputOrderAndCount()
        {
            var cards = new[] { new Card("AS"), new Card("2C"), new Card("KH") };
            var deck = new CardDeck(cards);

            Assert.Equal(3, deck.Count);
            Assert.Equal(cards, deck.Cards);
        }

        [Fact]
        public void CardDeck_PickQuantity_RemovesAndReturnsRequestedCards()
        {
            var deck = new CardDeck(false);

            var picked = deck.Pick(3);

            Assert.Equal(3, picked.Count);
            Assert.Equal(new[] { new Card("2C"), new Card("2H"), new Card("2S") }, picked);
            Assert.Equal(49, deck.Count);
        }

        [Fact]
        public void CardDeck_PickQuantityTwoReturnsTwoCards()
        {
            var deck = new CardDeck(false);

            Assert.Equal(2, deck.Pick(2).Count);
            Assert.Equal(50, deck.Count);
        }

        [Fact]
        public void CardDeck_NullCardsAreRejected()
        {
            Assert.Throws<ArgumentNullException>(() => new CardDeck(null!));
        }

        [Fact]
        public void CardDeck_ShufflePreservesAllCards()
        {
            var deck = new CardDeck(false);
            var originalCards = deck.Cards.ToHashSet();

            deck.Shuffle();

            Assert.Equal(52, deck.Count);
            Assert.Equal(originalCards, deck.Cards.ToHashSet());
        }

        [Fact]
        public void CardDeck_PickPastEndThrows()
        {
            var deck = new CardDeck(new[] { new Card("AC") });

            Assert.Throws<InvalidOperationException>(() => deck.Pick(2));
        }
    }
}
