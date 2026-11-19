namespace PokerEngine.Domain.Betting
{
    public enum PlayerActionType
    {
        Fold,
        Check,
        Call,
        Bet,
        Raise,
        AllIn
    }

    public sealed class PlayerAction
    {
        public PlayerAction(ushort playerId, PlayerActionType type, long amount)
        {
            if (playerId == 0)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId));
            }

            if (amount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "A player action amount cannot be negative.");
            }

            PlayerId = playerId;
            Type = type;
            Amount = amount;
        }

        public ushort PlayerId { get; }

        public PlayerActionType Type { get; }

        public long Amount { get; }
    }
}
