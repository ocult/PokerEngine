namespace PokerEngine.Domain.Betting
{
    public sealed class BettingPlayer
    {
        public BettingPlayer(ushort id, long stack)
        {
            if (id == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(id));
            }

            if (stack <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(stack));
            }

            Id = id;
            RemainingStack = stack;
            Status = BettingPlayerStatus.Pending;
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
    }
}
