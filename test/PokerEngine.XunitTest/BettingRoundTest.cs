using PokerEngine.Domain.Betting;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class BettingRoundTest
    {
        [Fact]
        public void BettingRound_StartsOpenWithRegisteredPlayers()
        {
            var round = CreateRound((1, 100), (2, 60));

            Assert.Equal(BettingRoundStatus.Open, round.Status);
            Assert.Equal(new ushort[] { 1, 2 }, round.Players.Select(player => player.Id));
            Assert.Equal(100, round.Players[0].RemainingStack);
            Assert.Equal(0, round.TotalContribution);
        }

        [Fact]
        public void BettingRound_RejectsInvalidPlayers()
        {
            Assert.Throws<ArgumentException>(() => new BettingRound(new[] { new BettingPlayer(1, 100) }));
            Assert.Throws<ArgumentException>(() => new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(1, 100)
            }));
            Assert.Throws<ArgumentNullException>(() => new BettingRound(null!));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BettingPlayer(0, 100));
            Assert.Throws<ArgumentOutOfRangeException>(() => new BettingPlayer(1, 0));

            BettingPlayer? nullPlayer = null;
            Assert.Throws<ArgumentException>(() => new BettingRound(new BettingPlayer?[]
            {
                new BettingPlayer(1, 100),
                nullPlayer
            }!));
        }

        [Fact]
        public void BettingRound_NextTracksCallAndRaiseFlow()
        {
            var round = CreateRound((1, 100), (2, 100), (3, 100));

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.Contribute(1, 25);
            Assert.Equal(25, round.BiggestContribution);

            Assert.Equal((ushort)2, round.Next()!.Id);
            round.Contribute(2, 25);
            Assert.Equal(25, round.BiggestContribution);

            Assert.Equal((ushort)3, round.Next()!.Id);
            round.Contribute(3, 50);
            Assert.Equal(50, round.BiggestContribution);

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.Contribute(1, 25);
            Assert.Equal(50, round.BiggestContribution);

            Assert.Equal((ushort)2, round.Next()!.Id);
            round.Contribute(2, 25);
            Assert.Equal(50, round.BiggestContribution);

            Assert.Null(round.Next());
        }

        [Fact]
        public void BettingRound_CallMatchesCurrentBiggestContribution()
        {
            var round = CreateRound((1, 100), (2, 100));

            round.Contribute(1, 25);
            round.Call(2);

            var player = round.Players.Single(player => player.Id == 2);
            Assert.Equal(25, player.Contribution);
            Assert.Equal(25, round.BiggestContribution);
        }

        [Fact]
        public void BettingRound_ContributeUpdatesPlayerAndAllInStatus()
        {
            var round = CreateRound((1, 100), (2, 60));

            round.Contribute(1, 100);

            var player = round.Players.Single(player => player.Id == 1);
            Assert.Equal(0, player.RemainingStack);
            Assert.Equal(100, player.Contribution);
            Assert.Equal(BettingPlayerStatus.AllIn, player.Status);
        }

        [Fact]
        public void BettingRound_FoldPreservesContributionAndRemovesEligibility()
        {
            var round = CreateRound((1, 100), (2, 100));
            round.Contribute(1, 40);
            round.Contribute(2, 40);
            round.Fold(2);
            round.Close();

            var pot = Assert.Single(round.GetPots());
            Assert.Equal(80, pot.Amount);
            Assert.Equal(new ushort[] { 1, 2 }, pot.Contributors);
            Assert.Equal(new ushort[] { 1 }, pot.EligiblePlayers);
        }

        [Fact]
        public void BettingRound_CloseRequiresEveryPlayerToAct()
        {
            var round = CreateRound((1, 100), (2, 100));
            round.Contribute(1, 40);

            Assert.Throws<InvalidOperationException>(() => round.Close());
            Assert.Equal(BettingRoundStatus.Open, round.Status);
        }

        [Fact]
        public void BettingRound_SettlementReturnsNextPlayersWithUpdatedStacks()
        {
            var round = CreateRound((1, 100), (2, 100));
            round.Contribute(1, 100);
            round.Contribute(2, 100);
            round.Close();

            var settlement = round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1 }
            });

            Assert.Equal(new ushort[] { 1 }, settlement.NextPlayers.Select(player => player.Id).ToArray());
            Assert.Equal(200, settlement.NextPlayers.Single(player => player.Id == 1).RemainingStack);
            Assert.Single(settlement.NextPlayers);
        }

        [Fact]
        public void BettingRound_RejectsInvalidActions()
        {
            var round = CreateRound((1, 100), (2, 100));

            Assert.Throws<ArgumentException>(() => round.Contribute(3, 10));
            Assert.Throws<ArgumentOutOfRangeException>(() => round.Contribute(1, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => round.Contribute(1, 101));

            round.Contribute(1, 25);
            round.Contribute(1, 10);
            round.Fold(1);
            Assert.Throws<InvalidOperationException>(() => round.Fold(1));
        }

        [Fact]
        public void BettingRound_BuildsMainAndSidePots()
        {
            var round = CreateRound((1, 100), (2, 60), (3, 30));
            round.Contribute(1, 100);
            round.Contribute(2, 60);
            round.Contribute(3, 30);
            round.Close();

            var pots = round.GetPots();

            Assert.Collection(
                pots,
                mainPot =>
                {
                    Assert.Equal(0, mainPot.Index);
                    Assert.Equal(90, mainPot.Amount);
                    Assert.Equal(new ushort[] { 1, 2, 3 }, mainPot.Contributors);
                    Assert.Equal(new ushort[] { 1, 2, 3 }, mainPot.EligiblePlayers);
                },
                sidePot =>
                {
                    Assert.Equal(1, sidePot.Index);
                    Assert.Equal(60, sidePot.Amount);
                    Assert.Equal(new ushort[] { 1, 2 }, sidePot.Contributors);
                },
                lastPot =>
                {
                    Assert.Equal(2, lastPot.Index);
                    Assert.Equal(40, lastPot.Amount);
                    Assert.Equal(new ushort[] { 1 }, lastPot.Contributors);
                    Assert.Equal(new ushort[] { 1 }, lastPot.EligiblePlayers);
                });
        }

        [Fact]
        public void BettingRound_SettlesPotsWithDifferentWinners()
        {
            var round = CreateRound((1, 100), (2, 60), (3, 30));
            round.Contribute(1, 100);
            round.Contribute(2, 60);
            round.Contribute(3, 30);
            round.Close();

            var settlement = round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 2 },
                [1] = new ushort[] { 1 },
                [2] = new ushort[] { 1 }
            });

            Assert.Equal(190, settlement.TotalPot);
            Assert.Equal(190, settlement.TotalPayout);
            Assert.Equal(90, settlement.Payouts.Single(payout => payout.PotIndex == 0).Amount);
            Assert.Equal(60, settlement.Payouts.Single(payout => payout.PotIndex == 1).Amount);
            Assert.Equal(40, settlement.Payouts.Single(payout => payout.PotIndex == 2).Amount);
            Assert.Equal(BettingRoundStatus.Settled, round.Status);
        }

        [Fact]
        public void BettingRound_SplitsTieAndDistributesRemainderByRegistrationOrder()
        {
            var round = CreateRound((1, 5), (2, 5), (3, 5));
            round.Contribute(1, 5);
            round.Contribute(2, 5);
            round.Contribute(3, 5);
            round.Close();

            var settlement = round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 3, 1, 2 }
            });

            Assert.Equal(new long[] { 5, 5, 5 }, settlement.Payouts
                .OrderBy(payout => payout.PlayerId)
                .Select(payout => payout.Amount));
        }

        [Fact]
        public void BettingRound_SplitsRemainderWithoutLosingChips()
        {
            var round = CreateRound((1, 31), (2, 31), (3, 31));
            round.Contribute(1, 31);
            round.Contribute(2, 31);
            round.Contribute(3, 31);
            round.Close();

            var settlement = round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1, 2 }
            });

            Assert.Equal(new long[] { 47, 46 }, settlement.Payouts
                .OrderBy(payout => payout.PlayerId)
                .Select(payout => payout.Amount));
            Assert.Equal(settlement.TotalPot, settlement.TotalPayout);
        }

        [Fact]
        public void BettingRound_RejectsInvalidSettlementAndActionsAfterClose()
        {
            var round = CreateRound((1, 10), (2, 10));
            round.Contribute(1, 10);
            round.Contribute(2, 10);
            round.Close();

            Assert.Throws<InvalidOperationException>(() => round.Contribute(1, 1));
            Assert.Throws<InvalidOperationException>(() => round.Fold(2));
            Assert.Throws<ArgumentException>(() => round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 3 }
            }));
            Assert.Throws<ArgumentException>(() => round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>()));
            Assert.Throws<ArgumentNullException>(() => round.Settle(null!));
        }

        [Fact]
        public void BettingRound_RejectsSettlementBeforeCloseAndRepeatedSettlement()
        {
            var round = CreateRound((1, 10), (2, 10));
            round.Contribute(1, 10);
            round.Contribute(2, 10);

            Assert.Throws<InvalidOperationException>(() => round.GetPots());
            Assert.Throws<InvalidOperationException>(() => round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1 }
            }));

            round.Close();
            round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1 }
            });
            Assert.Throws<InvalidOperationException>(() => round.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1 }
            }));
        }

        [Fact]
        public void BettingRound_RejectsClosingWhenEveryoneFolds()
        {
            var round = CreateRound((1, 10), (2, 10));
            round.Fold(1);
            round.Fold(2);

            Assert.Throws<InvalidOperationException>(() => round.Close());
        }

        [Fact]
        public void BettingRound_TestActionPlayerOrder() 
        {
            var round = CreateRound((1, 100), (2, 100), (3, 100), (4, 100));

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.Contribute(1, 10);

            Assert.Equal((ushort)2, round.Next()!.Id);
            round.Contribute(2, 20);

            Assert.Equal((ushort)3, round.Next()!.Id);
            round.Fold(3);

            Assert.Equal((ushort)4, round.Next()!.Id);
            round.Contribute(4, 30);

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.Fold(1);

            Assert.Equal((ushort)2, round.Next()!.Id);
            round.Contribute(2, 40);

            Assert.Equal((ushort)4, round.Next()!.Id);
            round.Call(4);

            Assert.Null(round.Next());
        }

        [Fact]
        public void BettingRound_ChainedRoundResetsPlayerStateForNextRound()
        {
            var round1 = CreateRound((1, 100), (2, 100), (3, 50));
            round1.Contribute(1, 50);
            round1.Fold(2);
            round1.Contribute(3, 50);
            round1.Close();

            var settlement = round1.Settle(new Dictionary<int, IReadOnlyCollection<ushort>>
            {
                [0] = new ushort[] { 1 }
            });

            Assert.Equal(2, settlement.NextPlayers.Count);

            var round2 = new BettingRound(settlement.NextPlayers);

            Assert.Equal(BettingRoundStatus.Open, round2.Status);
            Assert.Equal(0, round2.TotalContribution);
            Assert.Equal(0, round2.BiggestContribution);

            var p1 = round2.Players.Single(p => p.Id == 1);
            var p2 = round2.Players.Single(p => p.Id == 2);

            Assert.Equal(150, p1.RemainingStack);
            Assert.Equal(0, p1.Contribution);
            Assert.Equal(BettingPlayerStatus.Pending, p1.Status);

            Assert.Equal(100, p2.RemainingStack);
            Assert.Equal(0, p2.Contribution);
            Assert.Equal(BettingPlayerStatus.Pending, p2.Status);

            Assert.Equal((ushort)1, round2.Next()!.Id);
            round2.Contribute(1, 20);
            Assert.Equal((ushort)2, round2.Next()!.Id);
            round2.Call(2);
            Assert.Null(round2.Next());
        }

        [Fact]
        public void BettingRound_CallRejectsInvalidScenarios()
        {
            var round = CreateRound((1, 100), (2, 50));

            Assert.Throws<InvalidOperationException>(() => round.Call(1));
            Assert.Throws<ArgumentException>(() => round.Call(99));

            round.Contribute(1, 60);

            Assert.Throws<ArgumentOutOfRangeException>(() => round.Call(2));

            var round2 = CreateRound((1, 100), (2, 100));
            round2.Contribute(1, 30);
            round2.Fold(2);

            Assert.Throws<InvalidOperationException>(() => round2.Call(2));
        }

        [Fact]
        public void BettingRound_ContributeRejectsFoldedPlayer()
        {
            var round = CreateRound((1, 100), (2, 100));
            round.Fold(1);

            Assert.Throws<InvalidOperationException>(() => round.Contribute(1, 10));
        }

        [Fact]
        public void BettingRound_AllInSetsPlayerToAllInAndUpdatesContributions()
        {
            var round = CreateRound((1, 100), (2, 100));

            round.AllIn(1);

            var player = round.Players.Single(p => p.Id == 1);
            Assert.Equal(0, player.RemainingStack);
            Assert.Equal(100, player.Contribution);
            Assert.Equal(BettingPlayerStatus.AllIn, player.Status);
            Assert.Equal(100, round.BiggestContribution);
        }

        [Fact]
        public void BettingRound_AllInSkipsPlayerInSubsequentTurns()
        {
            var round = CreateRound((1, 50), (2, 100), (3, 100));

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.AllIn(1);

            Assert.Equal((ushort)2, round.Next()!.Id);
            round.Contribute(2, 80);

            Assert.Equal((ushort)3, round.Next()!.Id);
            round.Call(3);

            Assert.Null(round.Next());
            round.Close();
            Assert.Equal(BettingRoundStatus.Closed, round.Status);
        }

        [Fact]
        public void BettingRound_AllInRejectsInvalidScenarios()
        {
            var round = CreateRound((1, 100), (2, 100));
            round.Fold(1);

            Assert.Throws<InvalidOperationException>(() => round.AllIn(1));
            Assert.Throws<ArgumentException>(() => round.AllIn(99));

            round.AllIn(2);
            Assert.Throws<InvalidOperationException>(() => round.AllIn(2));

            round.Close();
            Assert.Throws<InvalidOperationException>(() => round.AllIn(2));
        }

        [Fact]
        public void BettingRound_CheckAllowsPlayersToPassWhenNoBet()
        {
            var round = CreateRound((1, 100), (2, 100), (3, 100));

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.Check(1);
            Assert.Equal(BettingPlayerStatus.Active, round.Players.Single(p => p.Id == 1).Status);

            Assert.Equal((ushort)2, round.Next()!.Id);
            round.Check(2);

            Assert.Equal((ushort)3, round.Next()!.Id);
            round.Check(3);

            Assert.Null(round.Next());
            round.Close();
            Assert.Equal(BettingRoundStatus.Closed, round.Status);
        }

        [Fact]
        public void BettingRound_CheckFollowedByBetAndCall()
        {
            var round = CreateRound((1, 100), (2, 100));

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.Check(1);

            Assert.Equal((ushort)2, round.Next()!.Id);
            round.Contribute(2, 30);

            Assert.Equal((ushort)1, round.Next()!.Id);
            round.Call(1);

            Assert.Null(round.Next());
            round.Close();
            Assert.Equal(BettingRoundStatus.Closed, round.Status);
        }

        [Fact]
        public void BettingRound_CheckRejectsWhenBetExistsOrFolded()
        {
            var round = CreateRound((1, 100), (2, 100));
            round.Contribute(1, 30);

            Assert.Throws<InvalidOperationException>(() => round.Check(2));
            Assert.Throws<ArgumentException>(() => round.Check(99));

            var round2 = CreateRound((1, 100), (2, 100));
            round2.Fold(1);
            Assert.Throws<InvalidOperationException>(() => round2.Check(1));

            round2.Check(2);
            round2.Close();
            Assert.Throws<InvalidOperationException>(() => round2.Check(2));
        }

        private static BettingRound CreateRound(params (ushort Id, long Stack)[] players)
        {
            return new BettingRound(players.Select(player => new BettingPlayer(player.Id, player.Stack)));
        }
    }
}
