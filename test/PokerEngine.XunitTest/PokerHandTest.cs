using PokerEngine.Domain.Models;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class PokerHandTest
    {
        private const ushort V1 = 1;
        private const ushort V2 = 2;
        private const ushort V3 = 3;
        private const ushort V4 = 4;

        [Theory]
        [InlineData(SuitEnum.Clubs)]
        [InlineData(SuitEnum.Hearts)]
        [InlineData(SuitEnum.Spades)]
        [InlineData(SuitEnum.Diamonds)]
        public void PokerHand_RoyalStraightFlush(SuitEnum se)
        {
            var suit = PokerHandTestHelper.GetCharSuit(se);
            var hand = new PokerHand($"T{suit},Q{suit},J{suit},K{suit},A{suit}");
            Assert.Equal(HandRankingEnum.RoyalStraightFlush, hand.HandRanking);
            Assert.Equal($"A royal straight flush of {se} [A{suit}, K{suit}, Q{suit}, J{suit}, T{suit}]", hand.ToString());
        }

        [Theory(DisplayName = "Check all straight flush possibilities")]
        [MemberData(nameof(PokerHandTestHelper.StraightPars), true, MemberType = typeof(PokerHandTestHelper))]
        public void PokerHand_StraightFlush(PokerHand hand)
        {
            Assert.Equal(HandRankingEnum.StraightFlush, hand.HandRanking);
        }

        [Fact(DisplayName = "Check straight flush A to 5")]
        public void PokerHand_StraightFlushAto5()
        {
            var hand = new PokerHand("2C, AC, 4C, 5C, 3C");
            Assert.Equal(HandRankingEnum.StraightFlush, hand.HandRanking);
            Assert.Collection(hand.Cards,
                              (c) => Assert.Equal(new Card("5C"), c),
                              (c) => Assert.Equal(new Card("4C"), c),
                              (c) => Assert.Equal(new Card("3C"), c),
                              (c) => Assert.Equal(new Card("2C"), c),
                              (c) => Assert.Equal(new Card("AC"), c));
        }

        [Theory(DisplayName = "Check straight A to 5")]
        [MemberData(nameof(PokerHandTestHelper.NotFlushSuits), MemberType = typeof(PokerHandTestHelper))]
        public void PokerHand_StraightAto5(SuitEnum se1, SuitEnum se2, SuitEnum se3, SuitEnum se4, SuitEnum se5)
        {
            var s1 = PokerHandTestHelper.GetCharSuit(se1);
            var s2 = PokerHandTestHelper.GetCharSuit(se2);
            var s3 = PokerHandTestHelper.GetCharSuit(se3);
            var s4 = PokerHandTestHelper.GetCharSuit(se4);
            var s5 = PokerHandTestHelper.GetCharSuit(se5);            
            if (s1 == 'X' || s2 == 'X' ||s3 == 'X' ||s4 == 'X' ||s5 == 'X' )
            {
                Console.Error.WriteLine($"Theory args are invalid [{se1},{se2},{se3},{se4},{se5}]");
                return;
            }

            var hand = new PokerHand($"2{s2}, A{s1}, 4{s4}, 5{s5}, 3{s3}");
            Assert.Equal(HandRankingEnum.Straight, hand.HandRanking);
            Assert.Collection(hand.Cards,
                              (c) => Assert.Equal(new Card($"5{s5}"), c),
                              (c) => Assert.Equal(new Card($"4{s4}"), c),
                              (c) => Assert.Equal(new Card($"3{s3}"), c),
                              (c) => Assert.Equal(new Card($"2{s2}"), c),
                              (c) => Assert.Equal(new Card($"A{s1}"), c));
        }

        [Theory(DisplayName = "Check all straight possibilities")]
        [MemberData(nameof(PokerHandTestHelper.StraightPars), false, MemberType = typeof(PokerHandTestHelper))]
        public void PokerHand_Straight(PokerHand hand)
        {
            Assert.Equal(HandRankingEnum.Straight, hand.HandRanking);
        }


        [Theory(DisplayName = "Check ranking and names")]
        [InlineData("TC,JC,QC,KC,AC", HandRankingEnum.RoyalStraightFlush, "A royal straight flush of Clubs")]
        [InlineData("TH,QH,JH,KH,AH", HandRankingEnum.RoyalStraightFlush, "A royal straight flush of Hearts")]
        [InlineData("JS,TS,QS,KS,AS", HandRankingEnum.RoyalStraightFlush, "A royal straight flush of Spades")]
        [InlineData("JD,TD,AD,QD,KD", HandRankingEnum.RoyalStraightFlush, "A royal straight flush of Diamonds")]
        [InlineData("JC,TC,9C,QC,KC", HandRankingEnum.StraightFlush, "A king-high straight flush of Clubs")]
        [InlineData("JH,TH,9H,QH,8H", HandRankingEnum.StraightFlush, "A queen-high straight flush of Hearts")]
        [InlineData("6S,TS,9S,7S,8S", HandRankingEnum.StraightFlush, "A ten-high straight flush of Spades")]
        [InlineData("3D,AD,4D,2D,5D", HandRankingEnum.StraightFlush, "A five-high straight flush of Diamonds")]
        [InlineData("AD,AH,AS,AC,6C", HandRankingEnum.FourOfKind, "A four of aces with a six kicker")]
        [InlineData("6D,6S,6H,6C,AH", HandRankingEnum.FourOfKind, "A four of sixes with a ace kicker")]
        public void PokerHandRankingName(string cards, HandRankingEnum rank, string name)
        {
            var hand = new PokerHand(cards);
            var fullName = $"{name} {hand.CardsString}";
            Assert.Equal(rank, hand.HandRanking);
            Assert.Equal(fullName, hand.ToString());
        }

        [Theory]
        [InlineData("AC, KC, JH, 9D, 3D", "AC, KC, JH, 9D, 6S", 1)]
        [InlineData("AC, KC, JH, 9D, 5D", "AC, KC, JH, 9D, 4S", -1)]
        [InlineData("AC, KC, JH, 9D, 4D", "AC, KC, JH, 9D, 4S", 0)]
        public void CompareTwoHands(string cardsA, string cardsB, int compare)
        {
            var handA = new PokerHand(cardsA);
            var handB = new PokerHand(cardsB);
            Assert.Equal(compare, handA.CompareTo(handB));
        }


        private static SuitEnum GetRandomSuitOrDefault(SuitEnum? defaultSuit = null)
        {
            var shuffle = new Random();
            return defaultSuit ?? (SuitEnum)shuffle.Next(1, 4);
        }
    }
}