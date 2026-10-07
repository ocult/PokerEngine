using System.IO;
using PokerEngine.Console;
using PokerEngine.Domain.Betting;
using PokerEngine.Domain.Models;
using PokerEngine.Domain.OmahaHoldem;
using PokerEngine.Domain.TexasHoldem;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class HoldemTableTest
    {
        [Fact]
        public void HoldemTable_TracksTurnOrder_AndBettingActionAtStart()
        {
            var table = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(3));

            Assert.Equal(HoldemStage.PreFlop, table.Stage);
            Assert.Equal(HoldemActionStage.Betting, table.CurrentAction);
            Assert.Equal((ushort)1, table.NextPlayerToAct);

            table.AdvanceTurn();
            Assert.Equal((ushort)2, table.NextPlayerToAct);

            Assert.True(table.CanAct(2));
            Assert.False(table.CanAct(1));
        }

        [Fact]
        public void HoldemTable_AdvancesToFlop_AndResetsToBettingAction()
        {
            var table = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(2));

            table.CloseBettingRound();
            table.AdvanceStreet();

            Assert.Equal(HoldemStage.Flop, table.Stage);
            Assert.Equal(3, table.Game.CommunityCards.Count);
            Assert.Equal(HoldemActionStage.Betting, table.CurrentAction);
            Assert.Equal((ushort)1, table.NextPlayerToAct);
        }

        [Fact]
        public void HoldemTable_TransitionsThroughRiver_AndReachesShowdown()
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

            table.CompleteHand();
            Assert.Equal(HoldemActionStage.Complete, table.CurrentAction);
        }

        [Fact]
        public void HoldemTable_RejectsAdvancingStreetBeforeClosingBettingRound()
        {
            var table = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(2));

            Assert.Throws<InvalidOperationException>(() => table.AdvanceStreet());
        }

        [Fact]
        public void HoldemTable_PassesCardWinnersToBettingSettlement()
        {
            var table = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(2));
            var round = new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100)
            });

            round.Contribute(1, 100);
            round.Contribute(2, 100);
            round.Close();

            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CloseBettingRound();
            table.AdvanceStreet();

            var bestHands = new Dictionary<ushort, PokerHand>
            {
                [1] = new PokerHand("AC, KC, QC, JC, TC")
            };

            var settlement = table.SettleHand(round, new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1 }
            }, bestHands);

            Assert.Equal(200, settlement.TotalPayout);
            Assert.Equal(BettingRoundStatus.Settled, round.Status);
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
        public void HoldemTable_IsGenericAcrossHoldemVariants()
        {
            var texasTable = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(2));
            var omahaTable = new HoldemTable<OmahaHoldemGame, OmahaHoldemPlayerCards>(new OmahaHoldemGame(2));

            Assert.Equal(HoldemStage.PreFlop, texasTable.Stage);
            Assert.Equal(HoldemStage.PreFlop, omahaTable.Stage);
            Assert.Equal(2, texasTable.Players);
            Assert.Equal(2, omahaTable.Players);
        }

        [Fact]
        public void PlayerAction_RejectsIllegalCallAndRaiseAcrossStreets()
        {
            var round = new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100)
            });

            round.Contribute(1, 20);

            Assert.Throws<InvalidOperationException>(() => round.ApplyAction(new PlayerAction(2, PlayerActionType.Call, 5)));
            Assert.Throws<InvalidOperationException>(() => round.ApplyAction(new PlayerAction(2, PlayerActionType.Raise, 19)));

            round.ApplyAction(new PlayerAction(2, PlayerActionType.Call, 0));
            Assert.Equal(20, round.Players.Single(player => player.Id == 2).Contribution);

            round.Close();

            var table = new HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards>(new TexasHoldemGame(2));
            Assert.Equal(HoldemStage.PreFlop, table.Stage);
            table.CloseBettingRound();
            table.AdvanceStreet();
            Assert.Equal(HoldemStage.Flop, table.Stage);
            table.CloseBettingRound();
            table.AdvanceStreet();
            Assert.Equal(HoldemStage.Turn, table.Stage);
            table.CloseBettingRound();
            table.AdvanceStreet();
            Assert.Equal(HoldemStage.River, table.Stage);
        }
    }
}
