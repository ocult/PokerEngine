using PokerEngine.Domain.Models;
using MSC = System.Console;

namespace PokerEngine.Console
{
    public static class EvaluateRunner
    {
        public static void Help()
        {
            MSC.WriteLine("EVALUATE: [cards] - Evaluates a 5-card poker hand (e.g., 'AH,KH,QH,JH,TH' or 'AS KS QS JS TS').");
        }

        public static void Run(string cards)
        {
            PokerHand hand = new(cards);
            MSC.WriteLine($"You have {hand}");
        }
    }
}
