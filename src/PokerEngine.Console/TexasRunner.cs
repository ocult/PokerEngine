using PokerEngine.Domain.Models;
using PokerEngine.Domain.TexasHoldem;
using MSC = System.Console;

namespace PokerEngine.Console
{
    public static class TexasRunner
    {
        public static void Help()
        {
            MSC.WriteLine("TEXAS: 'texas [players]' - Plays Texas Hold'em dealing hands, flop, turn, river, and showdown for [players] (2+).");
        }

        public static void Run(ushort players)
        {
            TexasHoldemGame game = new(players);
            HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards> table = new(game);

            foreach (KeyValuePair<ushort, TexasHoldemPlayerCards> player in game.PlayersCards)
            {
                MSC.WriteLine($"Player #{player.Key} have [{player.Value.FirstCard}, {player.Value.SecondCard}] in hand");
            }

            MSC.WriteLine("Press any key to continue to the table cards...");
            MSC.ReadLine();

            table.CloseBettingRound();
            table.AdvanceStreet();
            IReadOnlyList<Card> flop = game.CommunityCards;
            MSC.WriteLine($"Table flop is [{flop[0]}, {flop[1]}, {flop[2]}]");
            MSC.WriteLine("Press any key to continue to the turn card...");
            MSC.ReadLine();

            table.CloseBettingRound();
            table.AdvanceStreet();
            IReadOnlyList<Card> turn = game.CommunityCards;
            MSC.WriteLine($"Table turn is {turn[3]}");
            MSC.WriteLine("Press any key to continue to the river card...");
            MSC.ReadLine();

            table.CloseBettingRound();
            table.AdvanceStreet();
            IReadOnlyList<Card> river = game.CommunityCards;
            MSC.WriteLine($"Table river is {river[4]}");
            MSC.WriteLine("Press any key to continue to the showdown...");
            MSC.ReadLine();

            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CompleteHand();

            IReadOnlyList<KeyValuePair<ushort, PokerHand>> hands = game.GetBestHands();
            foreach (KeyValuePair<ushort, PokerHand> hand in hands)
            {
                string status = hand.Key == 0 ? "Table" : $"Player #{hand.Key}";
                MSC.WriteLine($"{status} best possible hand is {hand.Value}");
            }

            MSC.WriteLine("Press any key to goes to winner announcement...");
            MSC.ReadLine();
            PrintWinner(hands);
        }

        private static void PrintWinner(IReadOnlyList<KeyValuePair<ushort, PokerHand>> playersHands)
        {
            ushort win = playersHands.FirstOrDefault().Key;
            MSC.WriteLine(win == 0 ? "The table winner" : $"The winner is player #{win}".ToUpperInvariant());
            MSC.WriteLine("Ranked players hands:");
            foreach (KeyValuePair<ushort, PokerHand> item in playersHands)
            {
                MSC.WriteLine(item.Key == 0 ? $"Table has {item.Value}" : $"Player #{item.Key} has {item.Value}");
            }
        }
    }
}
