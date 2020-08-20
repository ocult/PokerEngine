using System;

namespace PokerEngine.Domain.Models
{
    public struct Card
    {
        public string Name { get; private set; }
        public string ValueName => GetValueName(Value);
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

        
        public static string GetValueName(ushort value)
        {
            return value switch
            {
                1 => "Ace",
                3 => "Three",
                5 => "Five",
                6 => "Sixe",
                10 => "Ten",
                11 => "Jack",
                12 => "Queen",
                13 => "King",
                14 => "Ace",
                _ => value.ToString(),
            };
        }
        public static string GetLowerValueName(ushort value)
        {
            return GetValueName(value).ToLowerInvariant();
        }

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || obj.GetType() != typeof(Card))
            {
                return false;
            }
            var card = (Card)obj;
            var value = Value == 1 ? 14 : Value;
            var cardValue = card.Value == 1 ? 14 : card.Value;
            return value == cardValue && Suit == card.Suit;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
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