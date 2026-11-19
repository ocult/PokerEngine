namespace PokerEngine.Domain.Betting
{
    public sealed class BettingPayout
    {
        internal BettingPayout(int potIndex, ushort playerId, long amount)
        {
            PotIndex = potIndex;
            PlayerId = playerId;
            Amount = amount;
        }

        public int PotIndex { get; }

        public ushort PlayerId { get; }

        public long Amount { get; }
    }
}
