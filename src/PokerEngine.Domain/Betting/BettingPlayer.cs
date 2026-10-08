namespace PokerEngine.Domain.Betting
{
    public sealed class BettingPlayer
    {
        public BettingPlayer(ushort id, long stack)
            : this(id, stack, BettingPlayerStatus.Pending)
        {
        }

        public BettingPlayer(ushort id, long stack, BettingPlayerStatus status)
            : this(id, stack, 0, status)
        {
        }

        internal BettingPlayer(ushort id, long stack, long contribution, BettingPlayerStatus status)
        {
            if (id == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (stack <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stack));
            }

            if (contribution < 0 || contribution > stack)
            {
                throw new ArgumentOutOfRangeException(nameof(contribution));
            }

            if (status != BettingPlayerStatus.Pending
                && status != BettingPlayerStatus.Active
                && status != BettingPlayerStatus.Folded
                && status != BettingPlayerStatus.AllIn)
            {
                throw new ArgumentOutOfRangeException(nameof(status));
            }

            Id = id;
            RemainingStack = stack - contribution;
            Contribution = contribution;
            Status = status;
        }

        public ushort Id { get; }

        public long RemainingStack { get; private set; }

        public long Contribution { get; private set; }

        public BettingPlayerStatus Status { get; private set; }

        internal void Contribute(long amount)
        {
            RemainingStack -= amount;
            Contribution += amount;
            Status = RemainingStack == 0
                ? BettingPlayerStatus.AllIn
                : BettingPlayerStatus.Active;
        }

        internal void Fold()
        {
            Status = BettingPlayerStatus.Folded;
        }

        internal void Check()
        {
            if (Status == BettingPlayerStatus.Pending)
            {
                Status = BettingPlayerStatus.Active;
            }
        }

        internal void Payout(long amount)
        {
            RemainingStack += amount;
        }

        internal void ResetForNextRound()
        {
            Contribution = 0;
            Status = BettingPlayerStatus.Pending;
        }
    }
}
