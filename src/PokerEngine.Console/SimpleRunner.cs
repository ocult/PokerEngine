using PokerEngine.Domain.Models;
using PokerEngine.Domain.SimpleGame;
using MSC = System.Console;

namespace PokerEngine.Console
{
    public static class SimpleRunner
    {
        public static void Help()
        {
            MSC.WriteLine("SIMPLE: 'simple [players]' - Deals 5 cards to each player for [players] (2+) and determines the winner.");
        }

        public static void Run(ushort players)
        {
            SimpleCardGame game = new(players);

            game.DealCards();

            foreach (KeyValuePair<ushort, IReadOnlyList<Card>> player in game.PlayersCards)
            {
                string cardsString = $"Player #{player.Key} have [";
                cardsString += string.Join(", ", player.Value.Select(card => card.ToString()));
                cardsString += "]";
                MSC.WriteLine(cardsString);
            }

            MSC.WriteLine("Final hands:");
            foreach (KeyValuePair<ushort, PokerHand> player in game.GetRankedHands())
            {
                MSC.WriteLine($"Player #{player.Key} has {player.Value}");
            }

            MSC.WriteLine($"Winner is Player #{game.GetWinnerPlayer()}");
        }
    }
}
