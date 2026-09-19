using PokerEngine.Console;
using MSC = System.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        string? cards = null;

        if (args != null && args.Length > 0)
        {
            cards = args.Length > 1 ? string.Join(',', args) : args[0];
            MSC.WriteLine($"Your cards are {cards}");
        }

        ReadCards(cards);

        static void ReadCards(string? cards = null)
        {
            while (string.IsNullOrWhiteSpace(cards))
            {
                MSC.WriteLine("What's yours cards? ");
                MSC.WriteLine(" Or quit/exit/q to exit, ");
                MSC.WriteLine(" or simple [players' number] to deal five cards, ");
                MSC.WriteLine(" or texas [players' number] to play texas holdem, ");
                MSC.WriteLine(" or omaha [players' number] to play omaha holdem");
                cards = MSC.ReadLine();
            }

            cards = cards.ToUpperInvariant().Trim();

            if (cards.StartsWith("QUIT") || cards.StartsWith("EXIT") || cards == "Q")
            {
                return;
            }

            try
            {
                if (cards.StartsWith("TEXAS"))
                {
                    string strPlayers = string.Join("", cards.Skip(5)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        throw new ArgumentException(nameof(players));
                    }

                    TexasRunner.Run(players);
                    ReadCards();
                    return;
                }

                if (cards.StartsWith("OMAHA"))
                {
                    string strPlayers = string.Join("", cards.Skip(5)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        throw new ArgumentException(nameof(players));
                    }

                    OmahaRunner.Run(players);
                    ReadCards();
                    return;
                }

                if (cards.StartsWith("SIMPLE"))
                {
                    string strPlayers = string.Join("", cards.Skip(6)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        throw new ArgumentException(nameof(players));
                    }

                    SimpleRunner.Run(players);
                    ReadCards();
                    return;
                }

                EvaluateRunner.Run(cards);
                ReadCards();
            }
            catch (Exception e)
            {
                MSC.WriteLine(e);
                ReadCards();
            }
        }
    }
}
