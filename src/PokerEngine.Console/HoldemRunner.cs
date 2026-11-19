using PokerEngine.Domain.Betting;
using PokerEngine.Domain.Holdem;
using PokerEngine.Domain.Models;
using MSC = System.Console;

namespace PokerEngine.Console
{
    public sealed class ConsoleRoundQuitException : OperationCanceledException
    {
    }

    public static class HoldemRunner
    {
        public static void WriteHelp(string message)
        {
            WriteWithColor(message, ConsoleColor.Gray);
        }

        public static void WriteInfo(string message)
        {
            WriteWithColor(message, ConsoleColor.Blue);
        }

        public static void WriteWaiting(string message)
        {
            WriteWithColor(message, ConsoleColor.Yellow);
        }

        private static void WriteWithColor(string message, ConsoleColor color)
        {
            ConsoleColor previous = MSC.ForegroundColor;
            MSC.ForegroundColor = color;
            try
            {
                MSC.WriteLine(message);
            }
            finally
            {
                MSC.ForegroundColor = previous;
            }
        }

        public static IReadOnlyList<BettingPlayer> PlayPreFlop<TGame, TPlayerCards>(
            HoldemTable<TGame, TPlayerCards> table,
            IReadOnlyList<BettingPlayer> players,
            Func<ushort, string>? playerCards = null,
            Func<ushort, string>? bestHandText = null)
            where TGame : HoldemGame<TPlayerCards>
            where TPlayerCards : HoldemPlayerCards
        {
            IReadOnlyList<BettingPlayer> nextPlayers = PlayBettingRound(players, playerCards, bestHandText);
            PrintCurrentPots(nextPlayers);
            table.CloseBettingRound();
            table.AdvanceStreet();
            return nextPlayers;
        }

        public static IReadOnlyList<BettingPlayer> PlayStreet<TGame, TPlayerCards>(
            HoldemTable<TGame, TPlayerCards> table,
            IReadOnlyList<BettingPlayer> players,
            string streetName,
            Func<IReadOnlyList<Card>> boardReader,
            Action<IReadOnlyList<Card>> boardPrinter,
            Func<ushort, string>? playerCards = null,
            Func<ushort, string>? bestHandText = null)
            where TGame : HoldemGame<TPlayerCards>
            where TPlayerCards : HoldemPlayerCards
        {
            IReadOnlyList<Card> board = boardReader();

            WriteWaiting($"Press any key to continue to the {streetName} betting round...");
            string? input = MSC.ReadLine();
            if (Program.IsQuitCommand(input))
            {
                WriteInfo("Quit requested. Leaving the hand.");
                throw new ConsoleRoundQuitException();
            }

            IReadOnlyList<BettingPlayer> nextPlayers = PlayBettingRound(players, playerCards, bestHandText, () => board, boardPrinter);
            PrintCurrentPots(nextPlayers);
            table.CloseBettingRound();
            table.AdvanceStreet();
            return nextPlayers;
        }

        public static void CompleteHand<TGame, TPlayerCards>(HoldemTable<TGame, TPlayerCards> table)
            where TGame : HoldemGame<TPlayerCards>
            where TPlayerCards : HoldemPlayerCards
        {
            if (table.Stage == HoldemStage.Complete)
            {
                table.CompleteHand();
                return;
            }

            table.CloseBettingRound();
            table.AdvanceStreet();
            table.CompleteHand();
        }

        public static IReadOnlyList<BettingPlayer> PlayBettingRound(
            IReadOnlyList<BettingPlayer> players,
            Func<ushort, string>? playerCards = null,
            Func<ushort, string>? bestHandText = null,
            Func<IReadOnlyList<Card>>? boardReader = null,
            Action<IReadOnlyList<Card>>? boardPrinter = null)
        {
            BettingRound round = new(players);

            while (true)
            {
                BettingPlayer? playerToAct = round.Next();
                if (playerToAct is null)
                {
                    break;
                }

                while (true)
                {
                    if (playerCards is not null)
                    {
                        WriteInfo($"Player #{playerToAct.Id} cards: {playerCards(playerToAct.Id)}");
                    }

                    if (boardReader is not null)
                    {
                        IReadOnlyList<Card> board = boardReader();
                        if (board.Count > 0)
                        {
                            boardPrinter?.Invoke(board);
                        }
                    }

                    if (bestHandText is not null)
                    {
                        string? currentBestHand = bestHandText(playerToAct.Id);
                        if (!string.IsNullOrWhiteSpace(currentBestHand))
                        {
                            WriteInfo($"Best hand: {currentBestHand}");
                        }
                    }

                    long amountToCall = Math.Max(0, round.BiggestContribution - playerToAct.Contribution);
                    WriteInfo($"Player #{playerToAct.Id} stack: {playerToAct.RemainingStack} | current bet: {round.BiggestContribution} | to call: {amountToCall}");
                    WriteWaiting("Action [CHECK [T]ABLE / [C]ALL / [F]OLD / [A]LLIN / <amount chips>] > ");
                    string? input = MSC.ReadLine();

                    if (string.IsNullOrWhiteSpace(input))
                    {
                        WriteInfo("Please enter a valid action.");
                        continue;
                    }

                    if (input.Equals("HELP", StringComparison.OrdinalIgnoreCase)
                        || input.Equals("H", StringComparison.OrdinalIgnoreCase))
                    {
                        PrintActionHelp();
                        continue;
                    }

                    if (Program.IsQuitCommand(input))
                    {
                        WriteInfo("Quit requested. Leaving the betting round.");
                        throw new ConsoleRoundQuitException();
                    }

                    try
                    {
                        PlayerAction action = ParseAction(playerToAct, round, input);
                        round.ApplyAction(action);
                        WriteInfo($"Player #{playerToAct.Id} action: {action.Type} {action.Amount}");
                        break;
                    }
                    catch (Exception ex)
                    {
                        WriteInfo(ex.Message);
                    }
                }
            }

            round.Close();
            return round.Players;
        }

        public static PlayerAction ParseAction(BettingPlayer playerToAct, BettingRound round, string input)
        {
            string normalized = input.Trim();

            if (normalized.Equals("CHECK", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("PASS", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("P", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("TABLE", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("T", StringComparison.OrdinalIgnoreCase))
            {
                return new PlayerAction(playerToAct.Id, PlayerActionType.Check, 0);
            }

            if (normalized.Equals("CALL", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("C", StringComparison.OrdinalIgnoreCase))
            {
                return new PlayerAction(playerToAct.Id, PlayerActionType.Call, 0);
            }

            if (normalized.Equals("FOLD", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("F", StringComparison.OrdinalIgnoreCase))
            {
                return new PlayerAction(playerToAct.Id, PlayerActionType.Fold, 0);
            }

            if (normalized.Equals("ALLIN", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("ALL-IN", StringComparison.OrdinalIgnoreCase)
                || normalized.Equals("A", StringComparison.OrdinalIgnoreCase))
            {
                return new PlayerAction(playerToAct.Id, PlayerActionType.AllIn, 0);
            }

            if (long.TryParse(normalized, out long chips) && chips > 0)
            {
                long amountToCall = Math.Max(0, round.BiggestContribution - playerToAct.Contribution);
                if ((amountToCall > 0 && chips == amountToCall)
                    || (amountToCall == 0 && chips == round.BiggestContribution))
                {
                    return new PlayerAction(playerToAct.Id, PlayerActionType.Call, 0);
                }

                if (round.BiggestContribution == 0 || playerToAct.Contribution == round.BiggestContribution)
                {
                    return new PlayerAction(playerToAct.Id, PlayerActionType.Bet, chips);
                }

                return new PlayerAction(playerToAct.Id, PlayerActionType.Raise, chips);
            }

            throw new ArgumentException("Invalid action. Use CHECK [T]ABLE, [C]ALL, [F]OLD, [A]LLIN, or a positive chip amount.");
        }

        public static void PrintActionHelp()
        {
            WriteHelp("Actions: ");
            WriteHelp("CHECK / PASS / TABLE / P / T --> to check or pass the action to the next player");
            WriteHelp("CALL / C --> to call the current bet or match the biggest contribution");
            WriteHelp("FOLD / F --> to fold and exit the hand");
            WriteHelp("ALL-IN / ALLIN / A --> to go all-in with the remaining chips");
            WriteHelp("<chips> --> to bet or raise by the specified amount");
        }

        public static void PrintCurrentPots(IReadOnlyList<BettingPlayer> players)
        {
            var levels = players
                .Where(player => player.Contribution > 0)
                .Select(player => player.Contribution)
                .Distinct()
                .OrderBy(level => level)
                .ToList();

            if (levels.Count == 0)
            {
                WriteInfo("Current pots: none");
                return;
            }

            long previousLevel = 0;
            for (int index = 0; index < levels.Count; index++)
            {
                long level = levels[index];
                IReadOnlyList<ushort> contributors = players
                    .Where(player => player.Contribution >= level)
                    .Select(player => player.Id)
                    .ToList();

                long amount = (level - previousLevel) * contributors.Count;
                IReadOnlyList<ushort> eligiblePlayers = contributors
                    .Where(playerId => players.Single(player => player.Id == playerId).Status != BettingPlayerStatus.Folded)
                    .ToList();

                WriteInfo($"Current pot #{index}: {amount} chips | contributors: {string.Join(", ", contributors.Select(playerId => $"#{playerId}"))} | eligible: {string.Join(", ", eligiblePlayers.Select(playerId => $"#{playerId}"))}");
                previousLevel = level;
            }
        }

        public static IReadOnlyList<BettingPlayer> SettleShowdown<TGame, TPlayerCards>(
            HoldemTable<TGame, TPlayerCards> table,
            IReadOnlyList<BettingPlayer> players,
            IReadOnlyList<KeyValuePair<ushort, PokerHand>> hands)
            where TGame : HoldemGame<TPlayerCards>
            where TPlayerCards : HoldemPlayerCards
        {
            ArgumentNullException.ThrowIfNull(table);
            ArgumentNullException.ThrowIfNull(players);
            ArgumentNullException.ThrowIfNull(hands);

            if (players.Count == 0)
            {
                return players;
            }

            var round = table.CreateBettingRound(players);
            round.Close();

            var winnersByPot = new Dictionary<int, IReadOnlyCollection<ushort>>();
            foreach (BettingPot pot in round.GetPots())
            {
                IReadOnlyList<ushort> eligible = pot.EligiblePlayers;
                if (eligible.Count == 0)
                {
                    continue;
                }

                if (eligible.Count == 1)
                {
                    winnersByPot[pot.Index] = new ushort[] { eligible[0] };
                    continue;
                }

                PokerHand bestHand = hands
                    .Where(hand => eligible.Contains(hand.Key))
                    .Select(hand => hand.Value)
                    .Max();

                ushort[] winners = hands
                    .Where(hand => eligible.Contains(hand.Key) && hand.Value.Equals(bestHand))
                    .Select(hand => hand.Key)
                    .OrderBy(playerId => playerId)
                    .ToArray();

                winnersByPot[pot.Index] = winners;
            }

            IReadOnlyDictionary<ushort, PokerHand> bestHands = hands
                .ToDictionary(hand => hand.Key, hand => hand.Value);

            BettingSettlement settlement = table.SettleHand(round, winnersByPot, bestHands);
            return settlement.NextPlayers;
        }

        public static void PrintWinner(
            IReadOnlyList<KeyValuePair<ushort, PokerHand>> playersHands,
            Func<ushort, string>? playerCards = null,
            IReadOnlyList<Card>? boardCards = null)
        {
            if (playersHands.Count == 0)
            {
                MSC.WriteLine("No winner hand data is available.");
                return;
            }

            if (boardCards is { Count: > 0 })
            {
                MSC.WriteLine($"Table cards: [{string.Join(", ", boardCards)}]");
            }

            ushort win = playersHands.FirstOrDefault().Key;
            MSC.WriteLine(win == 0 ? "The table winner" : $"The winner is player #{win}".ToUpperInvariant());
            MSC.WriteLine("Ranked players hands:");
            foreach (KeyValuePair<ushort, PokerHand> item in playersHands)
            {
                if (item.Key != 0 && playerCards is not null)
                {
                    MSC.WriteLine($"Player #{item.Key} cards: {playerCards(item.Key)}");
                }

                MSC.WriteLine(item.Key == 0 ? $"Table has {item.Value}" : $"Player #{item.Key} has {item.Value}");
            }
        }

        public static void PrintPlayerStacks(IReadOnlyList<BettingPlayer> players)
        {
            foreach (BettingPlayer player in players.OrderBy(player => player.Id))
            {
                MSC.WriteLine($"Player #{player.Id} chips: {player.RemainingStack}");
            }
        }

        public static void PrintPotWinner(
            IReadOnlyList<BettingPlayer> remainingPlayers,
            Func<ushort, string>? playerCards = null,
            IReadOnlyList<Card>? boardCards = null,
            IReadOnlyCollection<ushort>? cardWinnerIds = null)
        {
            if (remainingPlayers.Count == 0)
            {
                MSC.WriteLine("No active players remain. The pot is unassigned.");
                return;
            }

            PrintCurrentPots(remainingPlayers);

            if (boardCards is { Count: > 0 })
            {
                MSC.WriteLine($"Table cards: [{string.Join(", ", boardCards)}]");
            }

            if (remainingPlayers.Count == 1)
            {
                var winner = remainingPlayers[0];
                if (playerCards is not null)
                {
                    MSC.WriteLine($"Player #{winner.Id} cards: {playerCards(winner.Id)}");
                }

                MSC.WriteLine($"Player #{winner.Id} wins the pot by default.");
                return;
            }

            if (cardWinnerIds is null)
            {
                MSC.WriteLine("No hand winner is available for this pot.");
                return;
            }

            IReadOnlyList<ushort> winnerIds = cardWinnerIds
                .Distinct()
                .Where(playerId => remainingPlayers.Any(player => player.Id == playerId))
                .OrderBy(playerId => playerId)
                .ToList();

            if (winnerIds.Count == 0)
            {
                MSC.WriteLine("No eligible player matches the hand winner for this pot.");
                return;
            }

            foreach (ushort winnerId in winnerIds)
            {
                if (playerCards is not null)
                {
                    MSC.WriteLine($"Player #{winnerId} cards: {playerCards(winnerId)}");
                }
            }

            if (winnerIds.Count == 1)
            {
                MSC.WriteLine($"Player #{winnerIds[0]} wins the pot by hand ranking.");
                return;
            }

            MSC.WriteLine($"Players {string.Join(", ", winnerIds.Select(playerId => $"#{playerId}"))} split the pot by hand ranking.");
        }
    }
}
