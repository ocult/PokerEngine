using System;
using System.Collections.Generic;
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
            var suit = GetCharSuit(se);
            var hand = new PokerHand($"T{suit},Q{suit},J{suit},K{suit},A{suit}");
            Assert.Equal(HandRankingEnum.RoyalStraightFlush, hand.HandRanking);
            Assert.Equal($"A royal straight flush of {se} [A{suit}, K{suit}, Q{suit}, J{suit}, T{suit}]", hand.ToString());
        }

        [Theory(DisplayName = "Check all straight flush possibilities")]
        [MemberData(nameof(StraightPars), true)]
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
        [MemberData(nameof(NotFlushSuits))]
        public void PokerHand_StraightAto5(SuitEnum se1, SuitEnum se2, SuitEnum se3, SuitEnum se4, SuitEnum se5)
        {
            var s1 = GetCharSuit(se1);
            var s2 = GetCharSuit(se2);
            var s3 = GetCharSuit(se3);
            var s4 = GetCharSuit(se4);
            var s5 = GetCharSuit(se5);            
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
        [MemberData(nameof(StraightPars), false)]
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

        public static IEnumerable<object[]> NotFlushSuits()
        {
            uint s1 = 1;
            uint s2 = 1;
            uint s3 = 1;
            uint s4 = 1;
            uint s5 = 0;

            List<object[]> list = new List<object[]>();

            while (true)
            {
                ++s5;
                if (s5 == 5)
                {
                    s5 = 1;
                    ++s4;
                }
                else if (s4 == 5)
                {
                    s4 = 1;
                    ++s3;
                }
                else if (s3 == 5)
                {
                    s3 = 1;
                    ++s2;
                }
                else if (s2 == 5)
                {
                    s2 = 1;
                    ++s1;
                }
                else if (s1 == 5)
                {
                    break;
                }

                if (s1 != s2 ||
                    s1 != s3 ||
                    s1 != s4 ||
                    s1 != s5)
                {
                    list.Add(new object[]
                    {
                        (SuitEnum)s1,
                        (SuitEnum)s2,
                        (SuitEnum)s3,
                        (SuitEnum)s4,
                        (SuitEnum)s5
                    });
                }
            }
            return list;
        }

        #region Helpers
        private static char GetCharSuit(SuitEnum s1)
        {
            return GetCharSuit((uint)s1);
        }

        private static char GetCharSuit(uint s1)
        {
            return s1 switch
            {
                1u => 'C',
                2u => 'H',
                3u => 'S',
                4u => 'D',
                _ => 'X'
            };
        }

        public static IEnumerable<object[]> StraightPars(bool flush)
        {
            var suit = flush ? SuitEnum.Clubs : (SuitEnum?)null;
            foreach (var item in NotFlushSuits())
            {
                do
                {
                    var s1 = flush ? suit.Value : (SuitEnum)item[0];
                    var s2 = flush ? suit.Value : (SuitEnum)item[1];
                    var s3 = flush ? suit.Value : (SuitEnum)item[2];
                    var s4 = flush ? suit.Value : (SuitEnum)item[3];
                    var s5 = flush ? suit.Value : (SuitEnum)item[4];
                    for (ushort i = 5; i < (flush ? 14 : 15); ++i)
                    {
                        var cards = new Card[5]
                        {
                            new Card((ushort)(i - V1), s1),
                            new Card((ushort)(i - V2), s2),
                            new Card((ushort)(i - V3), s3),
                            new Card(i, s4),
                            new Card((ushort)(i - V4), s5)
                        };
                        yield return new object[] { new PokerHand(cards) };
                    }
                    if (flush) { ++suit; }
                }
                while (flush && (uint)suit <= 4u);

                if (flush)
                {
                    break;
                }
            }
        }

        private static SuitEnum GetRandomSuitOrDefault(SuitEnum? defaultSuit = null)
        {
            var shuffle = new Random();
            return defaultSuit ?? (SuitEnum)shuffle.Next(1, 4);
        }
        #endregion
    }
}