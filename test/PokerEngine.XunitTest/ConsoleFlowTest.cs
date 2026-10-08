using System.IO;
using PokerEngine.Console;
using PokerEngine.Domain.Betting;
using PokerEngine.Domain.Holdem;
using PokerEngine.Domain.Models;
using PokerEngine.Domain.TexasHoldem;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class ConsoleFlowTest
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

            var exception = Record.Exception(() => HoldemRunner.CompleteHand(table));
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

            var originalOut = System.Console.Out;
            try
            {
                using var writer = new StringWriter();
                System.Console.SetOut(writer);

                HoldemRunner.PrintCurrentPots(round.Players);

                var output = writer.ToString();
                Assert.Contains("Current pot #0", output);
                Assert.Contains("#1", output);
                Assert.Contains("#2", output);
                Assert.Contains("#3", output);
            }
            finally
            {
                System.Console.SetOut(originalOut);
            }
        }

        [Fact]
        public void HoldemRunner_PrintPotWinner_UsesCardWinnerCollection()
        {
            var originalOut = System.Console.Out;
            using var writer = new StringWriter();
            System.Console.SetOut(writer);

            try
            {
                HoldemRunner.PrintPotWinner(
                    new[]
                    {
                        new BettingPlayer(1, 100),
                        new BettingPlayer(2, 100)
                    },
                    playerId => $"cards-{playerId}",
                    new[]
                    {
                        new Card("AC"),
                        new Card("KC"),
                        new Card("QC"),
                        new Card("JC"),
                        new Card("TC")
                    },
                    new ushort[] { 1, 2 });
            }
            finally
            {
                System.Console.SetOut(originalOut);
            }

            string output = writer.ToString();
            Assert.Contains("split the pot by hand ranking", output, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("#1", output, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("#2", output, StringComparison.OrdinalIgnoreCase);
        }

        [Fact]
        public void HoldemRunner_SettlesWinnerPayoutAtShowdown()
        {
            var players = new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100)
            };

            var round = new BettingRound(players);
            round.Contribute(1, 40);
            round.Contribute(2, 40);
            round.Check(1);
            round.Check(2);
            round.Close();

            var bestHands = new[]
            {
                new KeyValuePair<ushort, PokerHand>(1, new PokerHand("AH, KH, QH, JH, TH")),
                new KeyValuePair<ushort, PokerHand>(2, new PokerHand("AS, KS, QS, JS, TS"))
            };

            var table = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(2));
            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();

            IReadOnlyList<BettingPlayer> settledPlayers = HoldemRunner.SettleShowdown(table, round.Players, bestHands);

            Assert.Equal(140, settledPlayers.Single(player => player.Id == 1).RemainingStack);
            Assert.Equal(60, settledPlayers.Single(player => player.Id == 2).RemainingStack);
        }

        [Fact]
        public void HoldemRunner_StoppesRoundWhenQuitActionIsEntered()
        {
            var originalIn = System.Console.In;
            try
            {
                System.Console.SetIn(new StringReader("q\n"));
                var players = new[]
                {
                    new BettingPlayer(1, 100),
                    new BettingPlayer(2, 100)
                };

                Assert.Throws<ConsoleRoundQuitException>(() => HoldemRunner.PlayBettingRound(players, playerId => $"cards-{playerId}"));
            }
            finally
            {
                System.Console.SetIn(originalIn);
            }
        }

        [Fact]
        public void BetRunner_ContinuesPotWhenWinnerSelectionIsBlank()
        {
            var round = new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100)
            });
            round.Contribute(1, 20);
            round.Contribute(2, 20);
            round.Close();

            var pot = round.GetPots().Single();

            Assert.Null(BetRunner.ResolvePotWinnerSelection(pot, string.Empty));
            Assert.Null(BetRunner.ResolvePotWinnerSelection(pot, "continue"));
        }

        [Fact]
        public void ConsoleFlow_MatchesCurrentHighestBetAfterInnerRoundReset()
        {
            var round = new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100)
            });

            round.Contribute(1, 10);
            round.Contribute(2, 10);
            round.Close();

            var continuation = BettingRound.CreateContinuationRound(round.Players);
            var playerToAct = continuation.Players.Single(player => player.Id == 1);

            var action = HoldemRunner.ParseAction(playerToAct, continuation, "10");

            Assert.Equal(PlayerActionType.Call, action.Type);
            Assert.Equal(0, action.Amount);
        }

        [Fact]
        public void ConsoleFlow_LastScenario_PreservesPotAndPaysWinnerAcrossInnerRounds()
        {
            var round = new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100),
                new BettingPlayer(3, 100)
            });

            round.Contribute(1, 10);
            round.Contribute(2, 10);
            round.Contribute(3, 10);
            round.Close();

            var flopRound = new BettingRound(round.Players);
            flopRound.Check(1);
            flopRound.Check(2);
            flopRound.Check(3);
            flopRound.Close();

            var turnRound = new BettingRound(flopRound.Players);
            turnRound.Contribute(1, 10);
            turnRound.Fold(2);
            turnRound.Contribute(3, 10);
            turnRound.Close();

            var riverRound = new BettingRound(turnRound.Players);
            riverRound.Check(1);
            riverRound.Check(3);
            riverRound.Close();

            var settlement = riverRound.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1 }
            });

            Assert.Equal(50, settlement.TotalPayout);
            Assert.Equal(130, settlement.NextPlayers.Single(player => player.Id == 1).RemainingStack);
            Assert.Equal(90, settlement.NextPlayers.Single(player => player.Id == 2).RemainingStack);
            Assert.Equal(80, settlement.NextPlayers.Single(player => player.Id == 3).RemainingStack);
        }

        [Fact]
        public void BetRunner_MatchesCurrentHighestBetAfterContinuationReset()
        {
            var originalIn = System.Console.In;
            var originalOut = System.Console.Out;

            try
            {
                System.Console.SetIn(new StringReader(
                    "10\n" +
                    "10\n" +
                    "10\n" +
                    "continue\n" +
                    "p\n" +
                    "p\n" +
                    "p\n" +
                    "continue\n" +
                    "10\n" +
                    "f\n" +
                    "10\n" +
                    "1\n" +
                    "q\n" +
                    "q\n"));

                using var writer = new StringWriter();
                System.Console.SetOut(writer);

                BetRunner.Run(3);

                string output = writer.ToString();
                Assert.Contains("Player #1 receives 50 chips", output, StringComparison.OrdinalIgnoreCase);
            }
            finally
            {
                System.Console.SetIn(originalIn);
                System.Console.SetOut(originalOut);
            }
        }
    }
}
