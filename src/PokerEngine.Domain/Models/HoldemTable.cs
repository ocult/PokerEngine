namespace PokerEngine.Domain.Models
{
    public sealed class HoldemTable<TGame, TPlayerCards>
        where TGame : HoldemGame<TPlayerCards>
        where TPlayerCards : HoldemPlayerCards
    {
        private readonly TGame _game;
        private readonly List<ushort> _playerOrder;
        private int _nextPlayerIndex;
        private bool _bettingClosed;

        public HoldemTable(TGame game)
        {
            _game = game ?? throw new ArgumentNullException(nameof(game));

            _playerOrder = Enumerable.Range(1, game.Players)
                .Select(player => (ushort)player)
                .ToList();

            _nextPlayerIndex = 0;
            _bettingClosed = false;
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

            _bettingClosed = true;
            CurrentAction = HoldemActionStage.CommunityCards;
            ResetTurn();
        }

        public void AdvanceStreet()
        {
            if (Stage == HoldemStage.Complete)
            {
                CurrentAction = HoldemActionStage.Complete;
                return;
            }

            if (!_bettingClosed)
            {
                CurrentAction = HoldemActionStage.Betting;
                return;
            }

            _game.Continue();
            _bettingClosed = false;
            ResetTurn();

            if (Stage == HoldemStage.Complete)
            {
                CurrentAction = HoldemActionStage.Complete;
                return;
            }

            CurrentAction = HoldemActionStage.Betting;
        }
    }
}
