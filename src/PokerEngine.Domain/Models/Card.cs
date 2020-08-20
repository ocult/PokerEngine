using System;

namespace PokerEngine.Domain.Models
{
    public struct Card : IComparable<Card>, IEquatable<Card>
    {
        public string Name { get; private set; }
        public string ValueName => GetValueName(Value);
        public string LowerValueName => ValueName.ToLowerInvariant();
        public SuitEnum Suit { get; }
        public ushort Value { get; }

        public Card(string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length != 2)
            {
                throw new ArgumentException(nameof(name));
            }
            name = name.ToUpper();

            Suit = name[1] switch
            {
                'C' => SuitEnum.Clubs,
                'H' => SuitEnum.Hearts,
                'S' => SuitEnum.Spades,
                'D' => SuitEnum.Diamonds,
                _ => throw new ArgumentException(nameof(name))
            };

            Value = name[0] switch
            {
                'A' => 14,
                'T' => 10,
                'J' => 11,
                'Q' => 12,
                'K' => 13,
                _ => ushort.Parse(name[0].ToString())
            };

            Name = name;
        }

        public Card(ushort value, SuitEnum suit)
        {
            if (value > 14)
            {
                throw new ArgumentException(nameof(value));
            }

            Value = value;
            Suit = suit;

            switch (value)
            {
                case 1:
                case 14:
                    Name = "A";
                    break;
                case 10:
                    Name = "T";
                    break;
                case 11:
                    Name = "J";
                    break;
                case 12:
                    Name = "Q";
                    break;
                case 13:
                    Name = "K";
                    break;
                default:
                    Name = Value.ToString();
                    break;
            }

            switch (Suit)
            {
                case SuitEnum.Clubs:
                    Name += "C";
                    break;
                case SuitEnum.Hearts:
                    Name += "H";
                    break;
                case SuitEnum.Diamonds:
                    Name += "D";
                    break;
                case SuitEnum.Spades:
                    Name += "S";
                    break;
            }
        }

        
        public static string GetValueName(ushort value, bool pluralize = false)
        {
            var name = value switch
            {
                1 => "Ace",
                2 => "Two",
                3 => "Three",
                4 => "Four",
                5 => "Five",
                6 => "Six",
                7 => "Seven",
                8 => "Eight",
                9 => "Nine",
                10 => "Ten",
                11 => "Jack",
                12 => "Queen",
                13 => "King",
                14 => "Ace",
                _ => value.ToString(),
            };
            return !pluralize ? name : value switch
            {
                6 => "Sixes",
                _ => $"{name}s"
            };
        }
        public static string GetLowerValueName(ushort value, bool pluralize = false)
        {
            return GetValueName(value, pluralize).ToLowerInvariant();
        }

        public override string ToString()
        {
            return Name;
        }

        public bool Equals(Card other)
        {
            var value = Value == 1 ? 14 : Value;
            var cardValue = other.Value == 1 ? 14 : other.Value;
            return value == cardValue && Suit == other.Suit;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Card))
            {
                return false;
            }
            return Equals((Card)obj);            
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public int CompareTo(Card other)
        {
            return this >= other ? this > other ? -1 : 0 : 1;
        }

        public static bool operator ==(Card cardA, Card cardB)
        {
            return cardA.Equals(cardB);
        }

        public static bool operator !=(Card cardA, Card cardB)
        {
            return !cardA.Equals(cardB);
        }

        public static bool operator >(Card cardA, Card cardB)
        {
            return cardA.Value > cardB.Value;
        }

        public static bool operator <(Card cardA, Card cardB)
        {
            return cardA.Value < cardB.Value;
        }

        public static bool operator >=(Card cardA, Card cardB)
        {
            return cardA.Value >= cardB.Value;
        }

        public static bool operator <=(Card cardA, Card cardB)
        {
            return cardA.Value <= cardB.Value;
        }
    }

    public enum SuitEnum
    {
        Clubs = 1,
        Hearts = 2,
        Spades = 3,
        Diamonds = 4
    }
}