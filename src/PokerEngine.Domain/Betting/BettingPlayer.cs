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
            InitialStack = stack;
            RemainingStack = stack;
            Status = BettingPlayerStatus.Pending;
        }

        public ushort Id { get; }

        public long InitialStack { get; }

        public long RemainingStack { get; private set; }

        public long Contribution { get; private set; }

        public BettingPlayerStatus Status { get; private set; }

        internal void Contribute(long amount)
        {
            if (amount <= 0 || amount > RemainingStack)
            {
                throw new ArgumentOutOfRangeException(nameof(amount));
            }

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
    }
}
