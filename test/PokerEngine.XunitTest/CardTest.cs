using PokerEngine.Domain.Models;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class CardTest
    {
        [Theory]
        [InlineData("ac", "AC", 14, SuitEnum.Clubs)]
        [InlineData("th", "TH", 10, SuitEnum.Hearts)]
        [InlineData("js", "JS", 11, SuitEnum.Spades)]
        [InlineData("qd", "QD", 12, SuitEnum.Diamonds)]
        [InlineData("9c", "9C", 9, SuitEnum.Clubs)]
        public void Card_FromName_NormalizesNameAndParsesValue(
            string name,
            string expectedName,
            ushort expectedValue,
            SuitEnum expectedSuit)
        {
            var card = new Card(name);

            Assert.Equal(expectedName, card.Name);
            Assert.Equal(expectedName, card.ToString());
            Assert.Equal(expectedValue, card.Value);
            Assert.Equal(expectedSuit, card.Suit);
        }

        [Theory]
        [InlineData(1, SuitEnum.Clubs, "AC")]
        [InlineData(10, SuitEnum.Hearts, "TH")]
        [InlineData(11, SuitEnum.Spades, "JS")]
        [InlineData(12, SuitEnum.Diamonds, "QD")]
        [InlineData(13, SuitEnum.Clubs, "KC")]
        [InlineData(14, SuitEnum.Hearts, "AH")]
        public void Card_FromValueAndSuit_CreatesExpectedName(
            ushort value,
            SuitEnum suit,
            string expectedName)
        {
            var card = new Card(value, suit);

            Assert.Equal(expectedName, card.Name);
            Assert.Equal(value, card.Value);
            Assert.Equal(suit, card.Suit);
        }

        [Fact]
        public void Card_ValueNames_ReturnSingularAndPluralForms()
        {
            Assert.Equal("Ace", Card.GetValueName(1));
            Assert.Equal("Ace", Card.GetValueName(14));
            Assert.Equal("Six", Card.GetValueName(6));
            Assert.Equal("Sixes", Card.GetValueName(6, true));
            Assert.Equal("Kings", Card.GetValueName(13, true));
            Assert.Equal("seven", Card.GetLowerValueName(7));
            Assert.Equal("queens", Card.GetLowerValueName(12, true));
            Assert.Equal("15", Card.GetValueName(15));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("A")]
        [InlineData("ACE")]
        [InlineData("AX")]
        public void Card_FromName_RejectsInvalidNames(string? name)
        {
            Assert.ThrowsAny<ArgumentException>(() => new Card(name!));
        }

        [Fact]
        public void Card_FromValue_RejectsValuesAboveAce()
        {
            Assert.Throws<ArgumentException>(() => new Card(15, SuitEnum.Clubs));
        }

        [Fact]
        public void Card_AceValuesAreEqualAndHashCodesMatch()
        {
            var aceByValue = new Card(1, SuitEnum.Spades);
            var aceByName = new Card("AS");

            Assert.Equal(aceByValue, aceByName);
            Assert.True(aceByValue == aceByName);
            Assert.False(aceByValue != aceByName);
            Assert.Equal(aceByValue.GetHashCode(), aceByName.GetHashCode());
            Assert.False(aceByValue.Equals("AS"));
        }

        [Fact]
        public void Card_ComparisonOperatorsAndCompareToOrderCardsByValue()
        {
            var high = new Card("KH");
            var low = new Card("3C");

            Assert.True(high > low);
            Assert.True(high >= low);
            Assert.True(low < high);
            Assert.True(low <= high);
            Assert.True(high >= new Card("KD"));
            Assert.True(low <= new Card("3D"));
            Assert.Equal(-1, high.CompareTo(low));
            Assert.Equal(1, low.CompareTo(high));
            Assert.Equal(0, high.CompareTo(new Card("KD")));
        }
    }
}