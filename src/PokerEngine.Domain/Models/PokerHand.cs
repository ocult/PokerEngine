using System;
using System.Collections.Generic;
using System.Linq;

namespace PokerEngine.Domain.Models
{
    public struct PokerHand
    {
        #region Basic
        public Card[] Cards { get; private set; }

        public PokerHand(string pokerHand)
        {
            if (string.IsNullOrWhiteSpace(pokerHand) || pokerHand.Count(c => c == ',') != 4)
            {
                throw new ArgumentException(nameof(pokerHand));
            }

            var cards = pokerHand.Split(",").Select(s => new Card(s.Trim()));
            this = new PokerHand(cards.ToArray());
        }

        public PokerHand(Card card1, Card card2, Card card3, Card card4, Card card5)
            : this(new Card[] { card1, card2, card3, card4, card5 }) { }

        public PokerHand(Card[] cards)
        {
            if (cards.Length != 5)
            {
                throw new ArgumentOutOfRangeException(nameof(cards));
            }
            Cards = Reorder(cards);
            HandRanking = HandRankingEnum.HighCard;
            _qualified = false;
            _fourOfKind = null;
            _highPair = null;
            _kicker = null;
            _secondKicker = null;
            _lastKicker = null;
            _lowPair = null;
            _pair = null;
            _threeOfKind = null;
            QualifyHand();
        }

        private static Card[] Reorder(Card[] cards)
        {
            return cards.OrderByDescending(c => c.Value).ToArray();
        }

        private static bool TryGetPokerHand(object obj, out PokerHand pokerHand)
        {
            pokerHand = default;
            if (obj == null || obj.GetType() != typeof(PokerHand))
            {
                return false;
            }
            pokerHand = (PokerHand)obj;
            return true;
        }

        public override bool Equals(object obj)
        {
            if (!TryGetPokerHand(obj, out var pokerHand))
            {
                return false;
            }
            return Cards[0] == pokerHand.Cards[0]
                   && Cards[1] == pokerHand.Cards[1]
                   && Cards[2] == pokerHand.Cards[2]
                   && Cards[3] == pokerHand.Cards[3]
                   && Cards[4] == pokerHand.Cards[4];
        }

        public override int GetHashCode()
        {
            return Cards[0].GetHashCode()
                   + Cards[1].GetHashCode()
                   + Cards[2].GetHashCode()
                   + Cards[3].GetHashCode()
                   + Cards[4].GetHashCode();
        }

        public override string ToString()
        {
            var cards = $"[{Cards[0]}, {Cards[1]}, {Cards[2]}, {Cards[3]}, {Cards[4]}]";
            return HandRanking switch
            {
                HandRankingEnum.RoyalStraightFlush => $"Royal straight flush of {this[0].Suit} {cards}",
                HandRankingEnum.StraightFlush => $"A {this[0].Name[0]}-high straight flush of {this[0].Suit} {cards}",
                _ => $"High card {cards}",
            };
        }


        public Card this[int i] => Cards[i];
        #endregion

        #region Qualify Ranking
        private bool _qualified;

        public HandRankingEnum HandRanking { get; private set; }

        private void QualifyHand()
        {
            Qualify(RoyalStraightFlush(), HandRankingEnum.RoyalStraightFlush);
            Qualify(StraightFlush(), HandRankingEnum.StraightFlush);
            Qualify(FourOfKind(), HandRankingEnum.FourOfKind);
            Qualify(FullHouse(), HandRankingEnum.FullHouse);
            Qualify(Flush(), HandRankingEnum.Flush);
            Qualify(Straight(), HandRankingEnum.Straight);
            Qualify(ThreeOfKind(), HandRankingEnum.ThreeOfKind);
            Qualify(TwoPairs(), HandRankingEnum.TwoPairs);
            Qualify(Pair(), HandRankingEnum.Pair);
        }

        private void Qualify(Func<PokerHand, PokerHand> qualify, HandRankingEnum ranking)
        {
            if (!_qualified)
            {
                this = qualify(this);

                if (_qualified)
                {
                    HandRanking = ranking;
                }
            }
        }

        private static Func<PokerHand, PokerHand> RoyalStraightFlush()
        {
            return (my) =>
            {
                my._qualified = my.CheckFlush()
                         && my[0].Value == 14
                         && my[1].Value == 13
                         && my[2].Value == 12
                         && my[3].Value == 11
                         && my[4].Value == 10;
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> StraightFlush()
        {
            return (my) =>
            {
                my._qualified = my.CheckFlush() && my.CheckStraight();
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> FourOfKind()
        {
            return (my) =>
            {
                var refKind = my[1].Value;
                my._qualified = my[2].Value == refKind
                             && my[3].Value == refKind;

                my._qualified = my._qualified && (my[0].Value == refKind || my[4].Value == refKind);
                if (my._qualified)
                {
                    my._fourOfKind = refKind;
                    my._kicker = my[my[0].Value == refKind ? 0 : 4];
                    my.Cards = Reorder(my[my[0].Value == refKind ? 4 : 0], my[1], my[2], my[3], my._kicker.Value);
                }
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> FullHouse()
        {
            return (my) =>
            {
                my._qualified = my[0].Value == my[1].Value
                             && my[3].Value == my[4].Value;

                my._qualified = my._qualified && (my[2].Value == my[0].Value
                                    || my[2].Value == my[4].Value);

                if (my._qualified)
                {
                    if (my[2].Value == my[0].Value)
                    {
                        my._threeOfKind = my[0].Value;
                        my._pair = my[3].Value;
                    }
                    else
                    {
                        my._threeOfKind = my[2].Value;
                        my._pair = my[0].Value;
                    }
                }
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> Flush()
        {
            return (my) =>
            {
                my._qualified = my.CheckFlush();
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> Straight()
        {
            return (my) =>
            {
                my._qualified = my.CheckStraight();
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> ThreeOfKind()
        {
            return (my) =>
            {
                my._qualified = false;
                if (my[2].Value == my[0].Value)
                {
                    my._threeOfKind = my[0].Value;
                    my._kicker = my[3];
                    my._secondKicker = my[4];
                }
                else if (my[2].Value == my[4].Value)
                {
                    my._qualified = true;
                    my._threeOfKind = my[4].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[1];
                    my.Cards = Reorder(my[2], my[3], my[4], my._kicker.Value, my._secondKicker.Value);
                }
                else if (my[2].Value == my[1].Value && my[2].Value == my[3].Value)
                {
                    my._qualified = true;
                    my._threeOfKind = my[2].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[4];
                    my.Cards = Reorder(my[1], my[2], my[3], my._kicker.Value, my._secondKicker.Value);
                }
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> TwoPairs()
        {
            return (my) =>
            {
                my._qualified = false;
                if (my[1].Value == my[2].Value && my[3].Value == my[4].Value)
                {
                    my._qualified = true;
                    my._kicker = my[0];
                    my._highPair = my[1].Value;
                    my._lowPair = my[3].Value;
                    my.Cards = Reorder(my[1], my[2], my[3], my[4], my._kicker.Value);
                }
                else if (my[0].Value == my[1].Value && my[3].Value == my[4].Value)
                {
                    my._qualified = true;
                    my._kicker = my[2];
                    my._highPair = my[0].Value;
                    my._lowPair = my[3].Value;
                    my.Cards = Reorder(my[0], my[1], my[3], my[4], my._kicker.Value);
                }
                else if (my[0].Value == my[1].Value && my[2].Value == my[3].Value)
                {
                    my._qualified = true;
                    my._kicker = my[4];
                    my._highPair = my[0].Value;
                    my._lowPair = my[2].Value;
                }
                return my;
            };
        }

        private static Func<PokerHand, PokerHand> Pair()
        {
            return (my) =>
            {
                my._qualified = false;
                if (my[0].Value == my[1].Value)
                {
                    my._qualified = true;
                    my._pair = my[0].Value;
                    my._kicker = my[2];
                    my._secondKicker = my[3];
                    my._lastKicker = my[4];
                }
                else if (my[1].Value == my[2].Value)
                {
                    my._qualified = true;
                    my._pair = my[1].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[3];
                    my._lastKicker = my[4];
                    my.Cards = Reorder(my[1], my[2], my._kicker.Value, my._secondKicker.Value, my._lastKicker.Value);
                }
                else if (my[2].Value == my[3].Value)
                {
                    my._qualified = true;
                    my._pair = my[2].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[1];
                    my._lastKicker = my[4];
                    my.Cards = Reorder(my[2], my[3], my._kicker.Value, my._secondKicker.Value, my._lastKicker.Value);
                }
                else if (my[3].Value == my[4].Value)
                {
                    my._qualified = true;
                    my._pair = my[3].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[1];
                    my._lastKicker = my[2];
                    my.Cards = Reorder(my[3], my[4], my._kicker.Value, my._secondKicker.Value, my._lastKicker.Value);
                }
                return my;
            };
        }
        private static Card[] Reorder(Card card1, Card card2, Card card3, Card card4, Card card5)
        {
            return new Card[] { card1, card2, card3, card4, card5 };
        }

        private bool CheckFlush()
        {
            var suit = this[0].Suit;
            return this[1].Suit == suit &&
                   this[2].Suit == suit &&
                   this[3].Suit == suit &&
                   this[4].Suit == suit;
        }

        private bool CheckStraight()
        {
            if (this[0].Value == 14
                && this[1].Value == 5
                && this[2].Value == 4
                && this[3].Value == 3
                && this[4].Value == 2)
            {
                Cards[0] = new Card(1, this[0].Suit);
                Cards = Reorder(Cards);
            }
            var init = this[0].Value;
            return this[1].Value == init - 1
                         && this[2].Value == init - 2
                         && this[3].Value == init - 3
                         && this[4].Value == init - 4;
        }
        #endregion

        #region Comparing
        private ushort? _fourOfKind;
        private Card? _kicker;
        private Card? _secondKicker;
        private Card? _lastKicker;
        private ushort? _threeOfKind;
        private ushort? _pair;
        private ushort? _highPair;
        private ushort? _lowPair;

        public static bool operator > (PokerHand handA, PokerHand handB)
        {
            if (handA.HandRanking != handB.HandRanking)
            {
                return handA.HandRanking > handB.HandRanking;
            }
            return handA.HandRanking switch
            {
                HandRankingEnum.RoyalStraightFlush => false,
                HandRankingEnum.FourOfKind => handA._fourOfKind > handB._fourOfKind 
                                              || handA._kicker > handB._kicker,
                HandRankingEnum.FullHouse => handA._threeOfKind > handB._threeOfKind 
                                             || handA._pair > handB._pair,
                HandRankingEnum.ThreeOfKind => handA._threeOfKind > handB._threeOfKind
                                               || handA._kicker > handB._kicker
                                               || handA._secondKicker > handB._secondKicker,
                HandRankingEnum.TwoPairs => handA._highPair > handB._highPair
                                            || handA._lowPair > handB._lowPair
                                            || handA._kicker > handB._kicker,
                HandRankingEnum.Pair => handA._pair > handB._pair
                                        || handA._kicker > handB._kicker
                                        || handA._secondKicker > handB._secondKicker
                                        || handA._lastKicker > handB._lastKicker,
                _ => handA[0] > handB[0]
                     || handA[1] > handB[1]
                     || handA[2] > handB[2]
                     || handA[3] > handB[3]
                     || handA[4] > handB[4]
            };
        }

        public static bool operator < (PokerHand handA, PokerHand handB)
        {
            return handB > handA;
        }

        public static bool operator >= (PokerHand handA, PokerHand handB)
        {
            if (handA.HandRanking != handB.HandRanking)
            {
                return handA.HandRanking >= handB.HandRanking;
            }
            return handA[0] >= handB[0]
                   || handA[1] >= handB[1]
                   || handA[2] >= handB[2]
                   || handA[3] >= handB[3]
                   || handA[4] >= handB[4];
        }

        public static bool operator <= (PokerHand handA, PokerHand handB)
        {
            return handB >= handA;
        }
    #endregion
}

public enum HandRankingEnum
{
    HighCard = 1,
    Pair = 2,
    TwoPairs = 3,
    ThreeOfKind = 4,
    Straight = 5,
    Flush = 6,
    FullHouse = 7,
    FourOfKind = 8,
    StraightFlush = 9,
    RoyalStraightFlush = 10
}
}
