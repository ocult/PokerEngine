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
            MSC.WriteLine("Enter cards or command (type 'help' for options, or 'q' to quit):");
            cards = MSC.ReadLine();
        }

        cards = cards.ToUpperInvariant().Trim();

        if (IsQuitCommand(cards))
        {
            return;                
        }

        if (cards.Equals("HELP", StringComparison.OrdinalIgnoreCase))
        {
            MSC.WriteLine("=== POKER ENGINE CONSOLE HELP ===");
            EvaluateRunner.Help();
            SimpleRunner.Help();
            TexasRunner.Help();
            BetRunner.Help();
            MSC.WriteLine("=================================");
            ReadCards();
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