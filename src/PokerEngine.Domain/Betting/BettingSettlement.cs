namespace PokerEngine.Domain.Betting
{
    public sealed class BettingSettlement
    {
        internal BettingSettlement(
            IReadOnlyList<BettingPot> pots,
            IReadOnlyList<BettingPayout> payouts,
            IReadOnlyList<BettingPlayer> nextPlayers)
        {
            Pots = pots;
            Payouts = payouts;
            NextPlayers = nextPlayers;
        }

        public IReadOnlyList<BettingPot> Pots { get; }

        public IReadOnlyList<BettingPayout> Payouts { get; }

        public IReadOnlyList<BettingPlayer> NextPlayers { get; }

        public long TotalPot => Pots.Sum(pot => pot.Amount);

        public long TotalPayout => Payouts.Sum(payout => payout.Amount);
    }
}
