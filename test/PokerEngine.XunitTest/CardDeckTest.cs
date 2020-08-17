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
            for (ushort i = 0, c = 2; c < 15; ++i, ++c)
            {
                Assert.Equal(new Card(c, SuitEnum.Clubs), deck[i]);
                Assert.Equal(new Card(c, SuitEnum.Hearts), deck[++i]);
                Assert.Equal(new Card(c, SuitEnum.Spades), deck[++i]);
                Assert.Equal(new Card(c, SuitEnum.Diamonds), deck[++i]);
            }
            Assert.Equal(new Card(2, SuitEnum.Clubs), deck[0]);
            Assert.Equal(new Card(14, SuitEnum.Diamonds), deck[51]);
        }

        [Fact]
        public void CardDeckTest_Order()
        {
            var deck = new CardDeck();
            deck.Order();
            Assert.Equal(52, deck.Count);
            ushort i = 0;
            for (ushort s = 1; s < 5; ++s)
            {
                SuitEnum suit = (SuitEnum)s;
                for (ushort c = 2; c < 15; ++c)
                {
                    Assert.Equal(new Card(c, suit), deck[i++]);
                }
            }
        }
    }
}
