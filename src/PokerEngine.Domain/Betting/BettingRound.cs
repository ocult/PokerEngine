namespace PokerEngine.Domain.Betting
{
    public sealed class BettingRound
    {
        private readonly Dictionary<ushort, BettingPlayer> _players;
        private readonly List<ushort> _playerOrder;

        public BettingRound(IEnumerable<BettingPlayer> players)
        {
            ArgumentNullException.ThrowIfNull(players);

            IReadOnlyList<BettingPlayer> playerList = players.ToList();
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
            BiggestContribution = 0;
            Status = BettingRoundStatus.Open;
        }

        public BettingRoundStatus Status { get; private set; }

        public IReadOnlyList<BettingPlayer> Players => _playerOrder
            .Select(playerId => _players[playerId])
            .ToList()
            .AsReadOnly();

        public long BiggestContribution { get; private set; }

        public long TotalContribution => _players.Values.Sum(player => player.Contribution);

        public BettingPlayer? Next()
        {
            if (Status != BettingRoundStatus.Open)
            {
                return null;
            }

            if (BiggestContribution == 0)
            {
                return _playerOrder
                    .Select(playerId => _players[playerId])
                    .FirstOrDefault(player => player.Status != BettingPlayerStatus.Folded);
            }

            return _playerOrder
                .Select(playerId => _players[playerId])
                .FirstOrDefault(player => player.Status != BettingPlayerStatus.Folded
                    && player.Contribution < BiggestContribution);
        }

        public void Contribute(ushort playerId, long amount)
        {
            EnsureOpen();
            BettingPlayer player = GetPlayer(playerId);

            if (player.Status == BettingPlayerStatus.Folded)
            {
                throw new InvalidOperationException("Folded players cannot contribute.");
            }

            if (amount <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "Contribution must be a positive number.");
            }

            if (amount > player.RemainingStack)
            {
                throw new ArgumentOutOfRangeException(nameof(amount), "The contribution cannot exceed the player's remaining stack.");
            }

            player.Contribute(amount);

            if (player.Contribution > BiggestContribution)
            {
                BiggestContribution = player.Contribution;
            }
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

        public void Call(ushort playerId)
        {
            EnsureOpen();
            BettingPlayer player = GetPlayer(playerId);
            if (player.Status == BettingPlayerStatus.Folded)
            {
                throw new InvalidOperationException("Folded players cannot call.");
            }

            long amountToCall = BiggestContribution - player.Contribution;
            if (amountToCall <= 0)
            {
                throw new InvalidOperationException("There is no current bet to call.");
            }

            if (amountToCall > player.RemainingStack)
            {
                throw new ArgumentOutOfRangeException(nameof(playerId), "The player cannot cover the call amount.");
            }

            player.Contribute(amountToCall);
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
            List<BettingPayout> payouts = [];

            foreach (BettingPot pot in pots)
            {
                if (!winnersByPot.TryGetValue(pot.Index, out IReadOnlyCollection<ushort>? winners)
                    || winners.Count == 0)
                {
                    throw new ArgumentException($"Winners are required for pot {pot.Index}.", nameof(winnersByPot));
                }

                IReadOnlyList<ushort> orderedWinners = winners
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

            foreach (BettingPlayer player in Players)
            {
                long payout = payouts
                    .Where(item => item.PlayerId == player.Id)
                    .Sum(item => item.Amount);

                if (payout > 0)
                {
                    player.Payout(payout);
                }
            }

            IReadOnlyList<BettingPlayer> nextPlayers = Players
                .Where(player => player.RemainingStack > 0)
                .ToList();

            Status = BettingRoundStatus.Settled;
            return new BettingSettlement(pots, payouts, nextPlayers);
        }

        private IReadOnlyList<BettingPot> BuildPots()
        {
            List<long> levels = _players.Values
                .Where(player => player.Contribution > 0)
                .Select(player => player.Contribution)
                .Distinct()
                .OrderBy(level => level)
                .ToList();
            List<BettingPot> pots = [];
            long previousLevel = 0;

            for (int index = 0; index < levels.Count; index++)
            {
                long level = levels[index];
                IReadOnlyList<ushort> contributors = _playerOrder
                    .Where(playerId => _players[playerId].Contribution >= level)
                    .ToList();
                long amount = (level - previousLevel) * contributors.Count;
                IReadOnlyList<ushort> eligiblePlayers = contributors
                    .Where(playerId => _players[playerId].Status != BettingPlayerStatus.Folded)
                    .ToList();
                pots.Add(new BettingPot(index, amount, contributors, eligiblePlayers));
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
            BettingPlayer clone = new (player.Id, player.RemainingStack);
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
