using System.Collections.Generic;
using System.Linq;
using PokerEngine.Domain.Models;

namespace PokerEngine.XunitTest
{
    public static class PokerHandTestHelper
    {
        public static CardDeck CreateOrderedDeck()
        {
            var cards = Enumerable.Range(2, 13)
                .SelectMany(value => new[]
                {
                    new Card((ushort)value, SuitEnum.Clubs),
                    new Card((ushort)value, SuitEnum.Hearts),
                    new Card((ushort)value, SuitEnum.Spades),
                    new Card((ushort)value, SuitEnum.Diamonds)
                });

            return new CardDeck(cards);
        }

        public static IEnumerable<object[]> NotFlushSuits()
        {
            var list = new List<object[]>();

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

                list.Add(
                [
                    (SuitEnum)s1,
                    (SuitEnum)s2,
                    (SuitEnum)s3,
                    (SuitEnum)s4,
                    (SuitEnum)s5
                ]);
            }

            return list;
        }

        public static char GetCharSuit(SuitEnum suit)
        {
            return GetCharSuit((uint)suit);
        }

        public static char GetCharSuit(uint suit)
        {
            return suit switch
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
                            new((ushort)(i - 1), suit),
                            new((ushort)(i - 2), suit),
                            new((ushort)(i - 3), suit),
                            new(i, suit),
                            new((ushort)(i - 4), suit)
                        };

                        list.Add([new PokerHand(cards)]);
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
                        new((ushort)(i - 1), s1),
                        new((ushort)(i - 2), s2),
                        new((ushort)(i - 3), s3),
                        new(i, s4),
                        new((ushort)(i - 4), s5)
                    };

                    list.Add([new PokerHand(cards)]);
                }
            }

            return list;
        }
    }
}
