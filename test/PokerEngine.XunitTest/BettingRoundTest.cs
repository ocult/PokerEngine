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

        private static BettingRound CreateRound(params (ushort Id, long Stack)[] players)
        {
            return new BettingRound(players.Select(player => new BettingPlayer(player.Id, player.Stack)));
        }
    }
}
