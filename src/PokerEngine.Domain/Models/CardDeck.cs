using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace PokerEngine.Domain.Models
{
    public class CardDeck
    {
        public CardDeck()
        {
            Cards = new List<Card>();
            for (ushort i = 2; i < 15; i++)
            {
                Cards.Add(new Card(i, SuitEnum.Clubs));
                Cards.Add(new Card(i, SuitEnum.Diamonds));
                Cards.Add(new Card(i, SuitEnum.Hearts));
                Cards.Add(new Card(i, SuitEnum.Spades));
            }
        }

        public IList<Card> Cards { get; private set; }

        public Card this[int index] => Cards[index];

        public int Count => Cards.Count;

        public void Order()
        {
            Cards = Cards.OrderBy(c => c.Value).OrderBy(c => c.Suit).ToList();
        }

        public void Shuffle()
        {
            var rng = new Random();
            int n = Cards.Count;
            while (n > 1)
            {
                --n;
                int k = rng.Next(n + 1);
                Card value = Cards[k];
                Cards[k] = Cards[n];
                Cards[n] = value;
            }
        }

        public void PowerShuffle()
        {

        }
    }
}