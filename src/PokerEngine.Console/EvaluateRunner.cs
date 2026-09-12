using PokerEngine.Domain.Models;
using MSC = System.Console;

namespace PokerEngine.Console
{
    public static class EvaluateRunner
    {
        public static void Run(string cards)
        {
            PokerHand hand = new(cards);
            MSC.WriteLine($"You have {hand}");
        }
    }
}
