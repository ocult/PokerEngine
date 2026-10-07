using System.IO;
using PokerEngine.Domain.Betting;
using PokerEngine.Domain.Models;
using PokerEngine.Domain.TexasHoldem;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class HoldemConsoleFlowTest
    {
        [Fact]
        public void HoldemRunner_CompleteHand_DoesNotReopenClosedBettingRoundAfterRiver()
        {
            var table = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(2));

            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();

            Assert.Equal(HoldemStage.Complete, table.Stage);
            Assert.Equal(HoldemActionStage.Showdown, table.CurrentAction);

            var exception = Record.Exception(() => global::PokerEngine.Console.HoldemRunner.CompleteHand(table));
            Assert.Null(exception);
            Assert.Equal(HoldemActionStage.Complete, table.CurrentAction);
        }

        [Fact]
        public void HoldemRunner_PrintCurrentPots_ReportsActivePlayerContributions()
        {
            var round = new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100),
                new BettingPlayer(3, 100)
            });

            round.Contribute(1, 20);
            round.Contribute(2, 20);
            round.Contribute(3, 40);

            var originalOut = global::System.Console.Out;
            try
            {
                using var writer = new StringWriter();
                global::System.Console.SetOut(writer);

                global::PokerEngine.Console.HoldemRunner.PrintCurrentPots(round.Players);

                var output = writer.ToString();
                Assert.Contains("Current pot #0", output);
                Assert.Contains("#1", output);
                Assert.Contains("#2", output);
                Assert.Contains("#3", output);
            }
            finally
            {
                global::System.Console.SetOut(originalOut);
            }
        }
    }
}
