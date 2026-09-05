namespace PokerEngine.Domain.Models
{
    public sealed class BettingRound
    {
        private readonly Dictionary<ushort, BettingPlayer> _players;
        private readonly List<ushort> _playerOrder;

        public BettingRound(IEnumerable<BettingPlayer> players)
        {
            ArgumentNullException.ThrowIfNull(players);

            var playerList = players.ToList();
            if (playerList.Count < 2)
            {
                throw new ArgumentException("At least two players are required.", nameof(players));
            }

            if (playerList.Any(player => player is null))
            {
                throw new ArgumentException("Players cannot contain null values.", nameof(players));
            }

            if (playerList.Select(player => player.Id).Distinct().Count() != playerList.Count)
            {
                throw new ArgumentException("Player IDs must be unique.", nameof(players));
            }

            _players = playerList.ToDictionary(player => player.Id, ClonePlayer);
            _playerOrder = playerList.Select(player => player.Id).ToList();
            Status = BettingRoundStatus.Open;
        }

        public BettingRoundStatus Status { get; private set; }

        public IReadOnlyList<BettingPlayer> Players => _playerOrder
            .Select(playerId => _players[playerId])
            .ToList()
            .AsReadOnly();

        public long TotalContribution => _players.Values.Sum(player => player.Contribution);

        public void Contribute(ushort playerId, long amount)
        {
            EnsureOpen();
            BettingPlayer player = GetPlayer(playerId);
            EnsurePending(player);
            player.Contribute(amount);
        }

        public void Fold(ushort playerId)
        {
            EnsureOpen();
            BettingPlayer player = GetPlayer(playerId);
            if (player.Status != BettingPlayerStatus.Pending
                && player.Status != BettingPlayerStatus.Active)
            {
                throw new InvalidOperationException("Only a pending or active player may fold.");
            }

            player.Fold();
        }

        public void Close()
        {
            EnsureOpen();
            if (_players.Values.Any(player => player.Status == BettingPlayerStatus.Pending))
            {
                throw new InvalidOperationException("Every player must act before closing the betting round.");
            }

            if (_players.Values.All(player => player.Status == BettingPlayerStatus.Folded))
            {
                throw new InvalidOperationException("At least one player must remain eligible.");
            }

            Status = BettingRoundStatus.Closed;
        }

        public IReadOnlyList<BettingPot> GetPots()
        {
            EnsureClosedOrSettled();
            return BuildPots();
        }

        public BettingSettlement Settle(IReadOnlyDictionary<int, IReadOnlyCollection<ushort>> winnersByPot)
        {
            ArgumentNullException.ThrowIfNull(winnersByPot);
            EnsureClosed();

            IReadOnlyList<BettingPot> pots = BuildPots();
            var payouts = new List<BettingPayout>();

            foreach (BettingPot pot in pots)
            {
                if (!winnersByPot.TryGetValue(pot.Index, out IReadOnlyCollection<ushort>? winners)
                    || winners.Count == 0)
                {
                    throw new ArgumentException($"Winners are required for pot {pot.Index}.", nameof(winnersByPot));
                }

                var orderedWinners = winners
                    .Distinct()
                    .OrderBy(playerId => _playerOrder.IndexOf(playerId))
                    .ToList();
                if (orderedWinners.Count != winners.Count
                    || orderedWinners.Any(playerId => !pot.EligiblePlayers.Contains(playerId)))
                {
                    throw new ArgumentException($"All winners for pot {pot.Index} must be eligible and unique.", nameof(winnersByPot));
                }

                long share = pot.Amount / orderedWinners.Count;
                long remainder = pot.Amount % orderedWinners.Count;
                for (int i = 0; i < orderedWinners.Count; i++)
                {
                    payouts.Add(new BettingPayout(
                        pot.Index,
                        orderedWinners[i],
                        share + (i < remainder ? 1 : 0)));
                }
            }

            Status = BettingRoundStatus.Settled;
            return new BettingSettlement(pots, payouts.AsReadOnly());
        }

        private IReadOnlyList<BettingPot> BuildPots()
        {
            var levels = _players.Values
                .Where(player => player.Contribution > 0)
                .Select(player => player.Contribution)
                .Distinct()
                .OrderBy(level => level)
                .ToList();
            var pots = new List<BettingPot>();
            long previousLevel = 0;

            for (int index = 0; index < levels.Count; index++)
            {
                long level = levels[index];
                var contributors = _playerOrder
                    .Where(playerId => _players[playerId].Contribution >= level)
                    .ToList();
                long amount = (level - previousLevel) * contributors.Count;
                var eligiblePlayers = contributors
                    .Where(playerId => _players[playerId].Status != BettingPlayerStatus.Folded)
                    .ToList();
                pots.Add(new BettingPot(index, amount, contributors.AsReadOnly(), eligiblePlayers.AsReadOnly()));
                previousLevel = level;
            }

            return pots.AsReadOnly();
        }

        private BettingPlayer GetPlayer(ushort playerId)
        {
            if (!_players.TryGetValue(playerId, out BettingPlayer? player))
            {
                throw new ArgumentException($"Player {playerId} is not part of the betting round.", nameof(playerId));
            }

            return player;
        }

        private static BettingPlayer ClonePlayer(BettingPlayer player)
        {
            var clone = new BettingPlayer(player.Id, player.InitialStack);
            if (player.Contribution > 0)
            {
                clone.Contribute(player.Contribution);
            }

            if (player.Status == BettingPlayerStatus.Folded)
            {
                clone.Fold();
            }

            return clone;
        }

        private static void EnsurePending(BettingPlayer player)
        {
            if (player.Status != BettingPlayerStatus.Pending)
            {
                throw new InvalidOperationException("Each player may act only once.");
            }
        }

        private void EnsureOpen()
        {
            if (Status != BettingRoundStatus.Open)
            {
                throw new InvalidOperationException("The betting round is not open.");
            }
        }

        private void EnsureClosed()
        {
            if (Status != BettingRoundStatus.Closed)
            {
                throw new InvalidOperationException("The betting round must be closed before settlement.");
            }
        }

        private void EnsureClosedOrSettled()
        {
            if (Status == BettingRoundStatus.Open)
            {
                throw new InvalidOperationException("The betting round must be closed before pots are calculated.");
            }
        }
    }
}
