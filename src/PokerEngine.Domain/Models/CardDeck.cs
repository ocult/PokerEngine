namespace PokerEngine.Domain.Models
{
    public class CardDeck
    {
        public CardDeck(bool shuffle = true)
            : this(CreateFreshDeck())
        {
            if (shuffle) PowerShuffle();
        }

        public CardDeck(IEnumerable<Card> cards)
        {
            Cards = new Queue<Card>(cards ?? throw new ArgumentNullException(nameof(cards)));
        }

        private static IEnumerable<Card> CreateFreshDeck()
        {
            List<Card> cards = new List<Card>();
            for (ushort i = 2; i < 15; i++)
            {
                cards.Add(new Card(i, SuitEnum.Clubs));
                cards.Add(new Card(i, SuitEnum.Hearts));
                cards.Add(new Card(i, SuitEnum.Spades));
                cards.Add(new Card(i, SuitEnum.Diamonds));
            }
            return cards;
        }

        public Queue<Card> Cards { get; private set; }

        public Card Pick() 
        {
            return Cards.Dequeue();
        }

        public IList<Card> Pick(ushort quantity = 2) 
        {
            List<Card> cards = new List<Card>();
            for (int i = 0; i < quantity; i++)
            {
                cards.Add(Cards.Dequeue());
            }            
            return cards;
        }

        public int Count => Cards.Count;

        public void Order()
        {
            Cards = new Queue<Card>(Cards.OrderBy(c => c.Value).OrderBy(c => c.Suit));
        }

        public void Shuffle()
        {
            Random rng = new Random();
            int n = Cards.Count;
            List<Card> cards = Cards.ToList();
            while (n > 1)
            {
                --n;
                int k = rng.Next(n + 1);
                Card value = cards[k];
                cards[k] = cards[n];
                cards[n] = value;
            }
            Cards = new Queue<Card>(cards);
        }

        public void PowerShuffle()
        {
            Random rng = new Random();
            int limit = rng.Next(Count);
            for (int i = 0; i < limit; ++i)
            {
                Shuffle();
            }
        }
    }
}