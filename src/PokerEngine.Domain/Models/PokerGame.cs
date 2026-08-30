namespace PokerEngine.Domain.Models
{
    public abstract class PokerGame
    {
        public abstract ushort Players { get; }

        public IReadOnlyList<KeyValuePair<ushort, PokerHand>> GetRankedHands()
        {
            return EvaluateBestHands()
                .OrderBy(pair => pair.Value)
                .ToList()
                .AsReadOnly();
        }

        public ushort GetWinnerPlayer()
        {
            return GetRankedHands().FirstOrDefault().Key;
        }

        protected abstract IDictionary<ushort, PokerHand> EvaluateBestHands();
    }
}
