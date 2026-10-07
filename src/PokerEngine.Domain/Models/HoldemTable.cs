using PokerEngine.Domain.Betting;

namespace PokerEngine.Domain.Models
{
    public sealed class HoldemTable<TGame, TPlayerCards>
        where TGame : HoldemGame<TPlayerCards>
        where TPlayerCards : HoldemPlayerCards
    {
        private readonly TGame _game;
        private readonly List<ushort> _playerOrder;
        private int _nextPlayerIndex;

        public HoldemTable(TGame game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));

            _playerOrder = Enumerable.Range(1, game.Players)
                .Select(player => (ushort)player)
                .ToList();

            _nextPlayerIndex = 0;
            CurrentAction = HoldemActionStage.Betting;
        }

        public TGame Game => _game;

        public ushort Players => _game.Players;

        public HoldemStage Stage => _game.Stage;

        public HoldemActionStage CurrentAction { get; private set; }

        public ushort NextPlayerToAct => _playerOrder[_nextPlayerIndex];

        public void AdvanceTurn()
        {
            if (_playerOrder.Count == 0)
            {
                throw new InvalidOperationException("The table must have at least one player.");
            }

            _nextPlayerIndex = (_nextPlayerIndex + 1) % _playerOrder.Count;
        }

        public void ResetTurn()
        {
            _nextPlayerIndex = 0;
        }

        public bool CanAct(ushort playerId)
        {
            return CurrentAction == HoldemActionStage.Betting
                && playerId == NextPlayerToAct;
        }

        public void CloseBettingRound()
        {
            if (Stage == HoldemStage.Complete)
            {
                throw new InvalidOperationException("The hand is already complete and no betting round is open.");
            }

            if (CurrentAction != HoldemActionStage.Betting)
            {
                throw new InvalidOperationException("Betting can only be closed while the table is awaiting player actions.");
            }

            CurrentAction = HoldemActionStage.CommunityCards;
            ResetTurn();
        }

        public void AdvanceStreet()
        {
            if (CurrentAction != HoldemActionStage.CommunityCards)
            {
                throw new InvalidOperationException("A new street can only be advanced after a betting round closes.");
            }

            _game.Continue();
            ResetTurn();

            if (Stage == HoldemStage.Complete)
            {
                CurrentAction = HoldemActionStage.Showdown;
                return;
            }

            CurrentAction = HoldemActionStage.Betting;
        }

        public void CompleteHand()
        {
            if (Stage != HoldemStage.Complete)
            {
                throw new InvalidOperationException("The hand can only be completed once the final street is resolved.");
            }

            CurrentAction = HoldemActionStage.Complete;
        }

        public BettingSettlement SettleHand(
            BettingRound round,
            IReadOnlyDictionary<int, IReadOnlyCollection<ushort>> winnersByPot,
            IReadOnlyDictionary<ushort, PokerHand>? bestHands = null)
        {
            ArgumentNullException.ThrowIfNull(round);
            ArgumentNullException.ThrowIfNull(winnersByPot);

            if (Stage != HoldemStage.Complete)
            {
                throw new InvalidOperationException("The hand must be complete before the betting round can be settled.");
            }

            IReadOnlyDictionary<ushort, PokerHand> evaluatedHands = bestHands is not null
                ? bestHands
                : _game.GetBestHands()
                    .ToDictionary(hand => hand.Key, hand => hand.Value);

            BettingSettlement settlement = round.Settle(winnersByPot, evaluatedHands);
            CurrentAction = HoldemActionStage.Complete;
            return settlement;
        }
    }
}
