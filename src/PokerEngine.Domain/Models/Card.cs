using System;

namespace PokerEngine.Domain.Models
{
    public struct Card
    {
        public string Name { get; private set; }
        public SuitEnum Suit {get; }
        public ushort Value { get; }

        public Card(string name)        
        {
            throw new NotImplementedException();
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

        public override string ToString()
        {
            return Name;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator == (Card cardA, Card cardB)
        {
            return cardA.Equals(cardB);
        }

        public static bool operator != (Card cardA, Card cardB)
        {
            return !cardA.Equals(cardB);
        }

        public static bool operator > (Card cardA, Card cardB)
        {
            return cardA.Value > cardB.Value;
        }

        public static bool operator < (Card cardA, Card cardB)
        {
            return cardA.Value < cardB.Value;
        }

        public static bool operator >= (Card cardA, Card cardB)
        {
            return cardA.Value >= cardB.Value;
        }

        public static bool operator <= (Card cardA, Card cardB)
        {
            return cardA.Value <= cardB.Value;
        }
    }

    public enum SuitEnum
    {
        Clubs,
        Hearts,
        Diamonds,
        Spades
    }
}