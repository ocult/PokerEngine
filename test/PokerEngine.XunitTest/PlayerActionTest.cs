using PokerEngine.Domain.Betting;
using Xunit;

namespace PokerEngine.XunitTest
{
    public class PlayerActionTest
    {
        [Fact]
        public void PlayerAction_StoresValidValues()
        {
            var action = new PlayerAction(7, PlayerActionType.Raise, 50);

            Assert.Equal((ushort)7, action.PlayerId);
            Assert.Equal(PlayerActionType.Raise, action.Type);
            Assert.Equal(50, action.Amount);
        }

        [Fact]
        public void PlayerAction_RejectsZeroPlayerId()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerAction(0, PlayerActionType.Call, 0));
        }

        [Fact]
        public void PlayerAction_RejectsNegativeAmount()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => new PlayerAction(1, PlayerActionType.Bet, -1));
        }

        [Fact]
        public void PlayerAction_AllowsZeroAmountForZeroChipActions()
        {
            var checkAction = new PlayerAction(1, PlayerActionType.Check, 0);
            var foldAction = new PlayerAction(1, PlayerActionType.Fold, 0);
            var callAction = new PlayerAction(1, PlayerActionType.Call, 0);

            Assert.Equal(0, checkAction.Amount);
            Assert.Equal(0, foldAction.Amount);
            Assert.Equal(0, callAction.Amount);
        }

        [Fact]
        public void BettingRound_ApplyAction_ValidatesLegalPokerMoves()
        {
            var round = new BettingRound(new[]
            {
                new BettingPlayer(1, 100),
                new BettingPlayer(2, 100)
            });

            round.Contribute(1, 20);

            round.ApplyAction(new PlayerAction(2, PlayerActionType.Call, 0));
            Assert.Equal(20, round.Players.Single(p => p.Id == 2).Contribution);

            round.ApplyAction(new PlayerAction(1, PlayerActionType.Check, 0));
            Assert.Equal(BettingPlayerStatus.Active, round.Players.Single(p => p.Id == 1).Status);

            round.ApplyAction(new PlayerAction(2, PlayerActionType.Raise, 30));
            Assert.Equal(50, round.Players.Single(p => p.Id == 2).Contribution);

            round.ApplyAction(new PlayerAction(1, PlayerActionType.Fold, 0));
            Assert.Equal(BettingPlayerStatus.Folded, round.Players.Single(p => p.Id == 1).Status);

            Assert.Throws<InvalidOperationException>(() => round.ApplyAction(new PlayerAction(2, PlayerActionType.Call, 5)));
            Assert.Throws<InvalidOperationException>(() => round.ApplyAction(new PlayerAction(2, PlayerActionType.Raise, 0)));
        }
    }
}
