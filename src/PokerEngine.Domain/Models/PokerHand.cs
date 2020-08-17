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
            Cards = cards.OrderByDescending(c => c).ToArray();
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
            return $"{Cards[0]}, {Cards[1]}, {Cards[2]}, {Cards[3]}, {Cards[4]}";
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

        private void Qualify(Func<PokerHand, bool> qualify, HandRankingEnum ranking)
        {
            if (!_qualified)
            {
                _qualified = qualify(this);

                if (_qualified)
                {
                    HandRanking = ranking;
                }
            }
        }

        private static Func<PokerHand, bool> RoyalStraightFlush()
        {
            return (my) => my.CheckFlush()
                         && my[0].Value == 14
                         && my[1].Value == 13
                         && my[2].Value == 12
                         && my[3].Value == 11
                         && my[4].Value == 10;
        }

        private static Func<PokerHand, bool> StraightFlush()
        {
            return (my) => my.CheckFlush() && my.CheckStraight();
        }

        private static Func<PokerHand, bool> FourOfKind()
        {
            return (my) =>
            {
                var refKind = my[1].Value;
                var result = my[2].Value == refKind
                             && my[3].Value == refKind;

                result = result && (my[0].Value == refKind || my[4].Value == refKind);
                if (result)
                {
                    my._fourOfKind = refKind;
                    my._kicker = my[my[0].Value == refKind ? 0 : 4];
                    my.Cards = Reorder(my[my[0].Value == refKind ? 4 : 0], my[1], my[2], my[3], my._kicker.Value);
                }
                return result;
            };
        }

        private static Func<PokerHand, bool> FullHouse()
        {
            return (my) =>
            {
                var result = my[0].Value == my[1].Value
                             && my[3].Value == my[4].Value;

                result = result && (my[2].Value == my[0].Value
                                    || my[2].Value == my[4].Value);

                if (result)
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
                return result;
            };
        }

        private static Func<PokerHand, bool> Flush()
        {
            return (my) => my.CheckFlush();
        }

        private static Func<PokerHand, bool> Straight()
        {
            return (my) => my.CheckStraight();
        }

        private static Func<PokerHand, bool> ThreeOfKind()
        {
            return (my) =>
            {
                var result = false;
                if (my[2].Value == my[0].Value)
                {
                    my._threeOfKind = my[0].Value;
                    my._kicker = my[3];
                    my._secondKicker = my[4];
                }
                else if (my[2].Value == my[4].Value)
                {
                    result = true;
                    my._threeOfKind = my[4].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[1];
                    my.Cards = Reorder(my[2], my[3], my[4], my._kicker.Value, my._secondKicker.Value);
                }
                else if (my[2].Value == my[1].Value && my[2].Value == my[3].Value)
                {
                    result = true;
                    my._threeOfKind = my[2].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[4];
                    my.Cards = Reorder(my[1], my[2], my[3], my._kicker.Value, my._secondKicker.Value);
                }
                return result;
            };
        }

        private static Func<PokerHand, bool> TwoPairs()
        {
            return (my) =>
            {
                var result = false;
                if (my[1].Value == my[2].Value && my[3].Value == my[4].Value)
                {
                    result = true;
                    my._kicker = my[0];
                    my._highPair = my[1].Value;
                    my._lowPair = my[3].Value;
                    my.Cards = Reorder(my[1], my[2], my[3], my[4], my._kicker.Value);
                }
                else if (my[0].Value == my[1].Value && my[3].Value == my[4].Value)
                {
                    result = true;
                    my._kicker = my[2];
                    my._highPair = my[0].Value;
                    my._lowPair = my[3].Value;
                    my.Cards = Reorder(my[0], my[1], my[3], my[4], my._kicker.Value);
                }
                else if (my[0].Value == my[1].Value && my[2].Value == my[3].Value)
                {
                    result = true;
                    my._kicker = my[4];
                    my._highPair = my[0].Value;
                    my._lowPair = my[2].Value;
                }
                return result;
            };
        }

        private static Func<PokerHand, bool> Pair()
        {
            return (my) =>
            {
                var result = false;
                if (my[0].Value == my[1].Value)
                {
                    result = true;
                    my._pair = my[0].Value;
                    my._kicker = my[2];
                    my._secondKicker = my[3];
                    my._lastKicker = my[4];
                }
                else if (my[1].Value == my[2].Value)
                {
                    result = true;
                    my._pair = my[1].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[3];
                    my._lastKicker = my[4];
                    my.Cards = Reorder(my[1], my[2], my._kicker.Value, my._secondKicker.Value, my._lastKicker.Value);
                }
                else if (my[2].Value == my[3].Value)
                {
                    result = true;
                    my._pair = my[2].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[1];
                    my._lastKicker = my[4];
                    my.Cards = Reorder(my[2], my[3], my._kicker.Value, my._secondKicker.Value, my._lastKicker.Value);
                }
                else if (my[3].Value == my[4].Value)
                {
                    result = true;
                    my._pair = my[3].Value;
                    my._kicker = my[0];
                    my._secondKicker = my[1];
                    my._lastKicker = my[2];
                    my.Cards = Reorder(my[3], my[4], my._kicker.Value, my._secondKicker.Value, my._lastKicker.Value);
                }
                return result;
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
                Cards.OrderByDescending(c => c.Value);
            }
            var init = this[0].Value;
            return this[1].Value == init - 1
                         && this[2].Value == init - 2
                         && this[3].Value == init - 3
                         && this[4].Value == init - 4;
        }
        #endregion

        #region Special private fields
        private ushort? _fourOfKind;
        private Card? _kicker;
        private Card? _secondKicker;
        private Card? _lastKicker;
        private ushort? _threeOfKind;
        private ushort? _pair;
        private ushort? _highPair;
        private ushort? _lowPair;
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
