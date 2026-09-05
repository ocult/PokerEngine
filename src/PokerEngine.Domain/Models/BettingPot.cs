namespace PokerEngine.Domain.Models
{
    public sealed class BettingPot
    {
        internal BettingPot(
            int index,
            long amount,
            IReadOnlyList<ushort> contributors,
            IReadOnlyList<ushort> eligiblePlayers)
        {
            Index = index;
            Amount = amount;
            Contributors = contributors;
            EligiblePlayers = eligiblePlayers;
        }

        public int Index { get; }

        public long Amount { get; }

        public IReadOnlyList<ushort> Contributors { get; }

        public IReadOnlyList<ushort> EligiblePlayers { get; }
    }
}
