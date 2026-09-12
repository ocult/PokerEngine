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
    }

    private static void ReadCards(string? cards = null)
    {
        while (string.IsNullOrWhiteSpace(cards))
        {
            MSC.WriteLine("What's yours cards? Or quit/exit/q to exit, or bet [players' number] to run a betting round, or simple [players' number] to deal five cards, or texas [players' number] to play texas holdem");
            cards = MSC.ReadLine();
        }

        cards = cards.ToUpperInvariant().Trim();

        if (IsQuitCommand(cards))
        {
            return;                
        }

        try
        {
            if (cards.StartsWith("BET"))
            {
                string strPlayers = string.Join("", cards.Skip(3)).Trim();
                if (!ushort.TryParse(strPlayers, out ushort players))
                {
                    throw new ArgumentException(nameof(players));
                }

                BetRunner.Run(players);
                ReadCards();
                return;
            }
            
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

    internal static bool IsQuitCommand(string? input)
    {
        return input != null
            && (input.Equals("QUIT", StringComparison.OrdinalIgnoreCase)
                || input.Equals("EXIT", StringComparison.OrdinalIgnoreCase)
                || input.Equals("Q", StringComparison.OrdinalIgnoreCase));
    }
    
}