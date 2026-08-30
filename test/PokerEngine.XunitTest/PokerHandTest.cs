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
            List<object[]> list = [];

            for (var s1 = 1; s1 <= 4; s1++)
            for (var s2 = 1; s2 <= 4; s2++)
            for (var s3 = 1; s3 <= 4; s3++)
            for (var s4 = 1; s4 <= 4; s4++)
            for (var s5 = 1; s5 <= 4; s5++)
            {
                if (s1 == s2 && s1 == s3 && s1 == s4 && s1 == s5)
                {
                    continue;
                }

                list.Add(new object[]
                {
                    (SuitEnum)s1,
                    (SuitEnum)s2,
                    (SuitEnum)s3,
                    (SuitEnum)s4,
                    (SuitEnum)s5
                });
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
            var list = new List<object[]>();

            if (flush)
            {
                foreach (var suit in new[] { SuitEnum.Clubs, SuitEnum.Hearts, SuitEnum.Spades, SuitEnum.Diamonds })
                {
                    for (ushort i = 5; i < 14; ++i)
                    {
                        var cards = new Card[5]
                        {
                            new Card((ushort)(i - V1), suit),
                            new Card((ushort)(i - V2), suit),
                            new Card((ushort)(i - V3), suit),
                            new Card(i, suit),
                            new Card((ushort)(i - V4), suit)
                        };

                        list.Add(new object[] { new PokerHand(cards) });
                    }
                }

                return list;
            }

            foreach (var item in NotFlushSuits())
            {
                var s1 = (SuitEnum)item[0]!;
                var s2 = (SuitEnum)item[1]!;
                var s3 = (SuitEnum)item[2]!;
                var s4 = (SuitEnum)item[3]!;
                var s5 = (SuitEnum)item[4]!;

                for (ushort i = 5; i < 15; ++i)
                {
                    var cards = new Card[5]
                    {
                        new Card((ushort)(i - V1), s1),
                        new Card((ushort)(i - V2), s2),
                        new Card((ushort)(i - V3), s3),
                        new Card(i, s4),
                        new Card((ushort)(i - V4), s5)
                    };

                    list.Add(new object[] { new PokerHand(cards) });
                }
            }

            return list;
        }

        [Fact]
        public void TexasHoldemGame_Continue_AdvancesStagesAndDealsCommunityCards()
        {
            var game = new TexasHoldemGame(2);

            Assert.Equal(TexasHoldemStage.PreFlop, game.Stage);
            Assert.Equal(2, game.PlayersCards.Count);
            Assert.Empty(game.CommunityCards);

            var flop = game.Continue();
            Assert.Equal(TexasHoldemStage.Flop, game.Stage);
            Assert.Equal(3, flop.Count);
            Assert.Equal(3, game.CommunityCards.Count);

            var turn = game.Continue();
            Assert.Equal(TexasHoldemStage.Turn, game.Stage);
            Assert.Equal(4, turn.Count);
            Assert.Equal(4, game.CommunityCards.Count);

            var river = game.Continue();
            Assert.Equal(TexasHoldemStage.River, game.Stage);
            Assert.Equal(5, river.Count);
            Assert.Equal(5, game.CommunityCards.Count);

            var complete = game.Continue();
            Assert.Equal(TexasHoldemStage.Complete, game.Stage);
            Assert.Equal(5, complete.Count);
        }

        [Fact]
        public void TexasHoldemGame_GetBestHands_ReturnsRankedResultsAfterRiver()
        {
            var game = new TexasHoldemGame(2);
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();
            _ = game.Continue();

            var hands = game.GetBestHands();

            Assert.Equal(3, hands.Count);
            Assert.Contains(hands, hand => hand.Key == 0);
            Assert.Contains(hands, hand => hand.Key == 1);
            Assert.Contains(hands, hand => hand.Key == 2);
        }

        [Fact]
        public void TexasHoldemGame_WithKnownDeck_BurnsAndDealsCommunityCardsInOrder()
        {
            var orderedCards = new[]
            {
                new Card(2, SuitEnum.Clubs),
                new Card(2, SuitEnum.Hearts),
                new Card(2, SuitEnum.Spades),
                new Card(2, SuitEnum.Diamonds),
                new Card(3, SuitEnum.Clubs),
                new Card(3, SuitEnum.Hearts),
                new Card(3, SuitEnum.Spades),
                new Card(3, SuitEnum.Diamonds),
                new Card(4, SuitEnum.Clubs),
                new Card(4, SuitEnum.Hearts),
                new Card(4, SuitEnum.Spades),
                new Card(4, SuitEnum.Diamonds),
                new Card(5, SuitEnum.Clubs),
                new Card(5, SuitEnum.Hearts),
                new Card(5, SuitEnum.Spades),
                new Card(5, SuitEnum.Diamonds),
                new Card(6, SuitEnum.Clubs),
                new Card(6, SuitEnum.Hearts),
                new Card(6, SuitEnum.Spades),
                new Card(6, SuitEnum.Diamonds),
                new Card(7, SuitEnum.Clubs),
                new Card(7, SuitEnum.Hearts),
                new Card(7, SuitEnum.Spades),
                new Card(7, SuitEnum.Diamonds),
                new Card(8, SuitEnum.Clubs),
                new Card(8, SuitEnum.Hearts),
                new Card(8, SuitEnum.Spades),
                new Card(8, SuitEnum.Diamonds),
                new Card(9, SuitEnum.Clubs),
                new Card(9, SuitEnum.Hearts),
                new Card(9, SuitEnum.Spades),
                new Card(9, SuitEnum.Diamonds),
                new Card(10, SuitEnum.Clubs),
                new Card(10, SuitEnum.Hearts),
                new Card(10, SuitEnum.Spades),
                new Card(10, SuitEnum.Diamonds),
                new Card(11, SuitEnum.Clubs),
                new Card(11, SuitEnum.Hearts),
                new Card(11, SuitEnum.Spades),
                new Card(11, SuitEnum.Diamonds),
                new Card(12, SuitEnum.Clubs),
                new Card(12, SuitEnum.Hearts),
                new Card(12, SuitEnum.Spades),
                new Card(12, SuitEnum.Diamonds),
                new Card(13, SuitEnum.Clubs),
                new Card(13, SuitEnum.Hearts),
                new Card(13, SuitEnum.Spades),
                new Card(13, SuitEnum.Diamonds),
                new Card(14, SuitEnum.Clubs),
                new Card(14, SuitEnum.Hearts),
                new Card(14, SuitEnum.Spades),
                new Card(14, SuitEnum.Diamonds)
            };

            var game = new TexasHoldemGame(5, new CardDeck(orderedCards));

            Assert.Equal(new Card(2, SuitEnum.Clubs), game.PlayersCards[1][0]);
            Assert.Equal(new Card(3, SuitEnum.Hearts), game.PlayersCards[1][1]);
            Assert.Equal(new Card(2, SuitEnum.Hearts), game.PlayersCards[2][0]);
            Assert.Equal(new Card(3, SuitEnum.Spades), game.PlayersCards[2][1]);
            Assert.Equal(new Card(2, SuitEnum.Spades), game.PlayersCards[3][0]);
            Assert.Equal(new Card(3, SuitEnum.Diamonds), game.PlayersCards[3][1]);
            Assert.Equal(new Card(2, SuitEnum.Diamonds), game.PlayersCards[4][0]);
            Assert.Equal(new Card(4, SuitEnum.Clubs), game.PlayersCards[4][1]);
            Assert.Equal(new Card(3, SuitEnum.Clubs), game.PlayersCards[5][0]);
            Assert.Equal(new Card(4, SuitEnum.Hearts), game.PlayersCards[5][1]);

            var flop = game.Continue();
            Assert.Equal(new Card(5, SuitEnum.Clubs), flop[0]);
            Assert.Equal(new Card(5, SuitEnum.Hearts), flop[1]);
            Assert.Equal(new Card(5, SuitEnum.Spades), flop[2]);
            Assert.Equal(3, flop.Count);
            Assert.Equal(3, game.CommunityCards.Count);

            var turn = game.Continue();
            Assert.Equal(new Card(6, SuitEnum.Clubs), turn[3]);
            Assert.Equal(4, turn.Count);

            var river = game.Continue();
            Assert.Equal(new Card(6, SuitEnum.Spades), river[4]);
            Assert.Equal(5, river.Count);
            Assert.Equal(TexasHoldemStage.River, game.Stage);
        }

        private static SuitEnum GetRandomSuitOrDefault(SuitEnum? defaultSuit = null)
        {
            var shuffle = new Random();
            return defaultSuit ?? (SuitEnum)shuffle.Next(1, 4);
        }
        #endregion
    }
}