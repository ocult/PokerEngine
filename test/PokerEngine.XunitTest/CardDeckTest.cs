using System;
using PokerEngine.Domain.Models;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class CardDeckTest
    {
        [Fact]
        public void CardDeckTest_New()
        {
            var deck = new CardDeck();
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
    }
}
