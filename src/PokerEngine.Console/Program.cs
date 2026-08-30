using PokerEngine.Domain.Models;
using MSC = System.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        CardDeck? _deck = null;
        string? cards = null;
        _deck = new CardDeck();
        _deck.PowerShuffle();

        if (args != null && args.Length > 0)
        {
            cards = args.Length > 1 ? string.Join(',', args) : args[0];
            MSC.WriteLine($"Your cards are {cards}");
        }
        ReadCards(cards);

        void ReadCards(string? cards = null)
        {
            while (string.IsNullOrWhiteSpace(cards))
            {
                MSC.WriteLine("What's yours cards? Or quit/exit/q to exit, or random [players] to get random hands, or texas [players] to play texas holdem");
                cards = MSC.ReadLine();
            }
            cards = cards.ToUpperInvariant();

            if (cards.StartsWith("QUIT") || cards.StartsWith("EXIT") || cards == "Q")
            {
                return;
            }
            try
            {
                if (cards.StartsWith("TEXAS"))
                {
                    string strPlayers = string.Join("", cards.Skip(6)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        throw new ArgumentException(nameof(players));
                    }
                    TexasHoldem(players);
                    ReadCards();
                    return;
                }
                else if (cards.StartsWith("RANDOM"))
                {
                    string strPlayers = string.Join("", cards.Skip(7)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        IList<Card> pickedCards = _deck.Pick(5);
                        cards = PokerHand.GetCardsString(pickedCards);
                    }
                    else
                    {
                        RandomCards(players);
                        ReadCards();
                        return;
                    }
                }
                PokerHand hand = new PokerHand(cards);
                MSC.WriteLine($"You have {hand}");
                ReadCards();
            }
            catch (Exception e)
            {
                if (_deck.Count < 5)
                {
                    _deck = new CardDeck();
                    _deck.PowerShuffle();
                }
                MSC.WriteLine(e);
                ReadCards();
            }
        }

        void TexasHoldem(ushort players)
        {
            TexasHoldemGame game = new TexasHoldemGame(players);

            foreach (KeyValuePair<ushort, IReadOnlyList<Card>> player in game.PlayersCards)
            {
                MSC.WriteLine($"Player #{player.Key} have [{player.Value[0]}, {player.Value[1]}] in hand");
            }

            MSC.WriteLine("Press any key to continue to the table cards...");
            MSC.ReadLine();

            IReadOnlyList<Card> flop = game.Continue();
            MSC.WriteLine($"Table flop is [{flop[0]}, {flop[1]}, {flop[2]}]");
            MSC.WriteLine("Press any key to continue to the turn card...");
            MSC.ReadLine();

            IReadOnlyList<Card> turn = game.Continue();
            MSC.WriteLine($"Table turn is {turn[3]}");
            MSC.WriteLine("Press any key to continue to the river card...");
            MSC.ReadLine();

            IReadOnlyList<Card> river = game.Continue();
            MSC.WriteLine($"Table river is {river[4]}");
            MSC.WriteLine("Press any key to continue to the showdown...");
            MSC.ReadLine();

            IReadOnlyList<KeyValuePair<ushort, PokerHand>> hands = game.GetBestHands();
            foreach (KeyValuePair<ushort, PokerHand> hand in hands)
            {
                string status = hand.Key == 0 ? "Table" : $"Player #{hand.Key}";
                MSC.WriteLine($"{status} best possible hand is {hand.Value}");
            }

            MSC.WriteLine("Press any key to goes to winner announcement...");
            MSC.ReadLine();
            PrintWinner(hands);
            _deck = new CardDeck();
            _deck.PowerShuffle();
        }

        void RandomCards(ushort players)
        {
            Dictionary<ushort, PokerHand> hands = new Dictionary<ushort, PokerHand>();
            for (ushort i = 1; i <= players; i++)
            {
                PokerHand playerHand = new PokerHand(_deck.Pick(5).ToArray());
                hands.Add(i, playerHand);
            }
            List<KeyValuePair<ushort, PokerHand>> playersHands = hands.OrderBy((a) => a.Value).ToList();
            PrintWinner(playersHands);
        }

        void PrintWinner(List<KeyValuePair<ushort, PokerHand>> playersHands)
        {
            ushort win = playersHands.FirstOrDefault().Key;
            MSC.WriteLine(win == 0 ? "The table winner" : $"The winner is player #{win}".ToUpperInvariant());
            MSC.WriteLine($"Ranked players hands:");
            foreach (KeyValuePair<ushort, PokerHand> item in playersHands)
            {
                MSC.WriteLine(item.Key == 0 ? $"Table has {item.Value}" : $"Player #{item.Key} has {item.Value}");
            }
        }
    }
}