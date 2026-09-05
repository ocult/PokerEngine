namespace PokerEngine.Domain.Models
{
    public sealed class BettingSettlement
    {
        internal BettingSettlement(
            IReadOnlyList<BettingPot> pots,
            IReadOnlyList<BettingPayout> payouts)
        {
            Pots = pots;
            Payouts = payouts;
        }

        public IReadOnlyList<BettingPot> Pots { get; }

        public IReadOnlyList<BettingPayout> Payouts { get; }

        public long TotalPot => Pots.Sum(pot => pot.Amount);

        public long TotalPayout => Payouts.Sum(payout => payout.Amount);
    }
}
