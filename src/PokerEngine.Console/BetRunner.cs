using PokerEngine.Domain.Betting;
using MSC = System.Console;

namespace PokerEngine.Console
{
    public static class BetRunner
    {
        public static void Help()
        {
            HoldemRunner.WriteHelp("BET: 'bet [players]' - Starts an interactive betting round for [players] (2+).");
        }

        private static void ActionHelp()
        {
            HoldemRunner.WriteHelp("  Actions during a betting round:");
            HoldemRunner.WriteHelp("    - 'CHECK'/'PASS'/'P'   : Check (pass) when no bet has been placed.");
            HoldemRunner.WriteHelp("    - 'CALL'/'C'           : Match the current highest bet.");
            HoldemRunner.WriteHelp("    - '[amount]'           : Enter total contribution to open or raise.");
            HoldemRunner.WriteHelp("    - 'ALLIN'/'ALL-IN'/'A' : Go all-in with all remaining chips.");
            HoldemRunner.WriteHelp("    - 'FOLD'/'F'           : Fold current hand.");
            HoldemRunner.WriteHelp("    - 'HELP'/'H'           : Show betting actions help.");
            HoldemRunner.WriteHelp("    - 'QUIT'/'EXIT'/'Q'    : Quit the betting round.");
        }

        public static void Run(ushort players)
        {
            if (players < 2)
            {
                throw new ArgumentException("Betting requires at least two players.", nameof(players));
            }

            IReadOnlyList<BettingPlayer> initialPlayers = Enumerable.Range(1, players)
                .Select(playerNumber => new BettingPlayer((ushort)playerNumber, 100))
                .ToList();

            Betting(initialPlayers);
        }

        public static IReadOnlyCollection<ushort>? ResolvePotWinnerSelection(BettingPot pot, string? rawWinners)
        {
            ArgumentNullException.ThrowIfNull(pot);

            if (pot.EligiblePlayers.Count == 1)
            {
                return new ushort[] { pot.EligiblePlayers[0] };
            }

            if (string.IsNullOrWhiteSpace(rawWinners)
                || rawWinners.Equals("CONTINUE", StringComparison.OrdinalIgnoreCase)
                || rawWinners.Equals("NEXT", StringComparison.OrdinalIgnoreCase)
                || rawWinners.Equals("POT", StringComparison.OrdinalIgnoreCase))
            {
                return null;
            }

            var parsedWinners = rawWinners
                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Select(winner =>
                {
                    if (!ushort.TryParse(winner, out ushort playerId))
                    {
                        throw new ArgumentException($"Invalid player id '{winner}'. Use numeric ids only.");
                    }

                    return playerId;
                })
                .ToArray();

            if (parsedWinners.Length == 0)
            {
                throw new ArgumentException("At least one winner must be informed.");
            }

            var duplicateWinners = parsedWinners
                .GroupBy(player => player)
                .Where(group => group.Count() > 1)
                .Select(group => group.Key)
                .ToList();

            if (duplicateWinners.Count > 0)
            {
                throw new ArgumentException($"Winner ids must be unique. Duplicates: {string.Join(", ", duplicateWinners.Select(playerId => $"#{playerId}"))}.");
            }

            var invalidWinners = parsedWinners
                .Where(playerId => !pot.EligiblePlayers.Contains(playerId))
                .ToArray();

            if (invalidWinners.Length > 0)
            {
                throw new ArgumentException($"Invalid winner ids for this pot: {string.Join(", ", invalidWinners.Select(playerId => $"#{playerId}"))}. Eligible players are {string.Join(", ", pot.EligiblePlayers.Select(playerId => $"#{playerId}"))}.");
            }

            return parsedWinners;
        }

        private static void Betting(IReadOnlyList<BettingPlayer> players, bool preserveCurrentPotState = false)
        {
            if (players.Count < 2)
            {
                throw new ArgumentException("Betting requires at least two players.", nameof(players));
            }

            BettingRound round = preserveCurrentPotState
                ? BettingRound.CreateContinuationRound(players)
                : new BettingRound(players);

            HoldemRunner.WriteInfo($"Started a betting round with {players.Count} players, each holding the current stack values.");

            BettingPlayer? playerToAct = round.Next();
            while (playerToAct != null)
            {
                while (true)
                {
                    long currentBiggestBet = round.BiggestContribution;
                    long amountToCall = Math.Max(0, currentBiggestBet - playerToAct.Contribution);

                    HoldemRunner.WriteInfo($"Player #{playerToAct.Id} stack: {playerToAct.RemainingStack} | Current highest bet: {currentBiggestBet}");
                    HoldemRunner.WriteWaiting("Enter action (or 'help'): ");
                    string? action = MSC.ReadLine();

                    if (string.IsNullOrWhiteSpace(action))
                    {
                        HoldemRunner.WriteInfo("Please enter a valid action (type 'help' for options).");
                        continue;
                    }

                    if (action.Equals("HELP", StringComparison.OrdinalIgnoreCase) 
                        || action.Equals("H", StringComparison.OrdinalIgnoreCase))
                    {
                        ActionHelp();
                        continue;
                    }

                    if (Program.IsQuitCommand(action))
                    {
                        QuitBettingRound(round);
                        return;
                    }

                    if (action.Equals("FOLD", StringComparison.OrdinalIgnoreCase) 
                        || action.Equals("F", StringComparison.OrdinalIgnoreCase))
                    {
                        round.Fold(playerToAct.Id);
                        HoldemRunner.WriteInfo($"Player #{playerToAct.Id} folded.");
                        break;
                    }

                    if (action.Equals("CHECK", StringComparison.OrdinalIgnoreCase) 
                        || action.Equals("PASS", StringComparison.OrdinalIgnoreCase)
                        || action.Equals("P", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            round.Check(playerToAct.Id);
                            HoldemRunner.WriteInfo($"Player #{playerToAct.Id} checked.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            HoldemRunner.WriteInfo(ex.Message);
                        }

                        continue;
                    }

                    if (action.Equals("CALL", StringComparison.OrdinalIgnoreCase) 
                        || action.Equals("C", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            round.Call(playerToAct.Id);
                            HoldemRunner.WriteInfo($"Player #{playerToAct.Id} called and contributed {amountToCall} chips.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            HoldemRunner.WriteInfo(ex.Message);
                        }

                        continue;
                    }

                    if (action.Equals("ALLIN", StringComparison.OrdinalIgnoreCase) 
                        || action.Equals("ALL-IN", StringComparison.OrdinalIgnoreCase) 
                        || action.Equals("A", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            long allInAmount = playerToAct.RemainingStack;
                            round.AllIn(playerToAct.Id);
                            HoldemRunner.WriteInfo($"Player #{playerToAct.Id} went all-in with {allInAmount} chips.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            HoldemRunner.WriteInfo(ex.Message);
                        }

                        continue;
                    }

                    if (!long.TryParse(action, out long targetContribution) || targetContribution <= 0)
                    {
                        HoldemRunner.WriteInfo("Contribution must be a positive integer.");
                        continue;
                    }

                    try
                    {
                        if (currentBiggestBet > 0 && targetContribution < currentBiggestBet)
                        {
                            throw new ArgumentOutOfRangeException(nameof(targetContribution), $"The target contribution must be at least {currentBiggestBet} to call or raise.");
                        }

                        long contribution = targetContribution - playerToAct.Contribution;
                        if (contribution <= 0)
                        {
                            throw new ArgumentOutOfRangeException(nameof(targetContribution), "The target contribution must be greater than the player's current contribution.");
                        }

                        round.Contribute(playerToAct.Id, contribution);
                        HoldemRunner.WriteInfo($"Player #{playerToAct.Id} contributed {contribution} chips and now has {targetContribution} total chips in the pot.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        HoldemRunner.WriteInfo(ex.Message);
                    }
                }

                playerToAct = round.Next();
            }

            round.Close();
            IReadOnlyList<BettingPot> pots = round.GetPots();

            HoldemRunner.WriteInfo("Betting round closed. Pots:");
            foreach (BettingPot pot in pots)
            {
                string eligiblePlayers = pot.EligiblePlayers.Count == 0
                    ? "none"
                    : string.Join(", ", pot.EligiblePlayers.Select(playerId => $"#{playerId}"));

                HoldemRunner.WriteInfo($"Pot #{pot.Index}: {pot.Amount} chips | contributors: {string.Join(", ", pot.Contributors.Select(playerId => $"#{playerId}"))} | eligible: {eligiblePlayers}");
            }

            var winnersByPot = new Dictionary<int, IReadOnlyCollection<ushort>>();
            foreach (BettingPot pot in pots)
            {
                while (true)
                {
                    HoldemRunner.WriteInfo($"Pot #{pot.Index} is contested by players: {string.Join(", ", pot.EligiblePlayers.Select(playerId => $"#{playerId}"))}.");
                    HoldemRunner.WriteWaiting("Inform the winning player id(s) for this pot (comma separated, or press Enter/'continue' to keep the pot open for the next round): ");
                    string? rawWinners = MSC.ReadLine();

                    if (Program.IsQuitCommand(rawWinners))
                    {
                        MSC.WriteLine("Quit requested. Exiting the betting flow.");
                        return;
                    }

                    try
                    {
                        IReadOnlyCollection<ushort>? winners = ResolvePotWinnerSelection(pot, rawWinners);
                        if (winners is null)
                        {
                            HoldemRunner.WriteInfo("Pot continues to the next betting round without a final winner.");
                            IReadOnlyList<BettingPlayer> continuedPlayers = round.Players
                                .Where(player => player.RemainingStack > 0)
                                .ToList();

                            if (continuedPlayers.Count < 2)
                            {
                                HoldemRunner.WriteInfo("Betting ended because fewer than two players still have chips.");
                                return;
                            }

                            HoldemRunner.WriteInfo("Starting a new betting round with the current players and chip values.");
                            foreach (BettingPlayer player in continuedPlayers)
                            {
                                HoldemRunner.WriteInfo($"Player #{player.Id} starts with {player.RemainingStack} chips.");
                            }

                            Betting(continuedPlayers, preserveCurrentPotState: true);
                            return;
                        }

                        winnersByPot[pot.Index] = winners;
                        break;
                    }
                    catch (Exception ex)
                    {
                        MSC.WriteLine(ex.Message);
                    }
                }
            }

            BettingSettlement settlement = round.Settle(winnersByPot);

            HoldemRunner.WriteInfo("Settlement payouts:");
            foreach (BettingPayout payout in settlement.Payouts.OrderBy(payout => payout.PotIndex).ThenBy(payout => payout.PlayerId))
            {
                HoldemRunner.WriteInfo($"Pot #{payout.PotIndex}: Player #{payout.PlayerId} receives {payout.Amount} chips.");
            }

            HoldemRunner.WriteInfo($"Total pot: {settlement.TotalPot}; total payouts: {settlement.TotalPayout}.");

            IReadOnlyList<BettingPlayer> nextPlayers = settlement.NextPlayers;

            if (nextPlayers.Count < 2)
            {
                HoldemRunner.WriteInfo("Betting ended because fewer than two players still have chips.");
                return;
            }

            HoldemRunner.WriteInfo("Starting a new betting round with the current players and chip values.");
            foreach (BettingPlayer player in nextPlayers)
            {
                HoldemRunner.WriteInfo($"Player #{player.Id} starts with {player.RemainingStack} chips.");
            }
            Betting(nextPlayers);
        }

        private static bool CanContinueBettingRound(IReadOnlyList<BettingPlayer> players)
        {
            return players
                .Count(player => player.RemainingStack > 0 && player.Status != BettingPlayerStatus.Folded) > 1;
        }

        private static void QuitBettingRound(BettingRound round)
        {
            HoldemRunner.WriteInfo("Betting round quit. Current players and pots:");

            foreach (BettingPlayer player in round.Players)
            {
                HoldemRunner.WriteInfo($"Player #{player.Id}: status={player.Status}, remaining={player.RemainingStack}, contribution={player.Contribution}");
            }

            PrintCurrentPots(round);

            if (CanContinueBettingRound(round.Players))
            {
                HoldemRunner.WriteInfo("A new betting round can start because more than one player still has chips.");
            }
            else
            {
                HoldemRunner.WriteInfo("A new betting round cannot start because fewer than two players still have chips.");
            }
        }

        private static void PrintCurrentPots(BettingRound round)
        {
            var levels = round.Players
                .Where(player => player.Contribution > 0)
                .Select(player => player.Contribution)
                .Distinct()
                .OrderBy(level => level)
                .ToList();

            if (levels.Count == 0)
            {
                HoldemRunner.WriteInfo("Current pots: none");
                return;
            }

            long previousLevel = 0;
            for (int index = 0; index < levels.Count; index++)
            {
                long level = levels[index];
                var contributors = round.Players
                    .Where(player => player.Contribution >= level)
                    .Select(player => player.Id)
                    .ToList();

                long amount = (level - previousLevel) * contributors.Count;
                var eligiblePlayers = contributors
                    .Where(playerId => round.Players.Single(player => player.Id == playerId).Status != BettingPlayerStatus.Folded)
                    .ToList();

                HoldemRunner.WriteInfo($"Current pot #{index}: {amount} chips | contributors: {string.Join(", ", contributors.Select(playerId => $"#{playerId}"))} | eligible: {string.Join(", ", eligiblePlayers.Select(playerId => $"#{playerId}"))}");
                previousLevel = level;
            }
        }
    }
}
