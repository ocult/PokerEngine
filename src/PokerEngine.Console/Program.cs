using PokerEngine.Domain.Betting;
using PokerEngine.Domain.Models;
using PokerEngine.Domain.SimpleGame;
using PokerEngine.Domain.TexasHoldem;
using MSC = System.Console;

internal class Program
{
    private static void Main(string[] args)
    {
        CardDeck? _deck = null;
        string? cards = null;
        _deck = new CardDeck();
        _deck.PowerShuffle();

        if (args != null && args.Length > 0)
        {
            cards = args.Length > 1 ? string.Join(',', args) : args[0];
            MSC.WriteLine($"Your cards are {cards}");
        }
        ReadCards(cards);

        void ReadCards(string? cards = null)
        {
            while (string.IsNullOrWhiteSpace(cards))
            {
                MSC.WriteLine("What's yours cards? Or quit/exit/q to exit, or bet [players' number] to run a betting round, or simple [players' number] to deal five cards, or texas [players' number] to play texas holdem");
                cards = MSC.ReadLine();
            }
            cards = cards.ToUpperInvariant();

            if (IsQuitCommand(cards))
            {
                return;                
            }

            try
            {
                if (cards.StartsWith("BET"))
                {
                    string strPlayers = string.Join("", cards.Skip(3)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        throw new ArgumentException(nameof(players));
                    }
                    Bet(players);
                    ReadCards();
                    return;
                }
                
                if (cards.StartsWith("TEXAS"))
                {
                    string strPlayers = string.Join("", cards.Skip(6)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        throw new ArgumentException(nameof(players));
                    }
                    TexasHoldem(players);
                    ReadCards();
                    return;
                }

                if (cards.StartsWith("SIMPLE"))
                {
                    string strPlayers = string.Join("", cards.Skip(6)).Trim();
                    if (!ushort.TryParse(strPlayers, out ushort players))
                    {
                        throw new ArgumentException(nameof(players));
                    }
                    SimpleGame(players);
                    ReadCards();
                    return;
                }

                PokerHand hand = new(cards);
                MSC.WriteLine($"You have {hand}");
                ReadCards();
            }
            catch (Exception e)
            {
                if (_deck.Count < 5)
                {
                    _deck = new CardDeck();
                    _deck.PowerShuffle();
                }
                MSC.WriteLine(e);
                ReadCards();
            }
        }


        bool IsQuitCommand(string? input)
        {
            return input != null
                && (input.Equals("QUIT", StringComparison.OrdinalIgnoreCase)
                    || input.Equals("EXIT", StringComparison.OrdinalIgnoreCase)
                    || input.Equals("Q", StringComparison.OrdinalIgnoreCase));
        }

        bool CanContinueBettingRound(IReadOnlyList<BettingPlayer> players)
        {
            return players
                .Count(player => player.RemainingStack > 0 && player.Status != BettingPlayerStatus.Folded) > 1;
        }

        void QuitBettingRound(BettingRound round)
        {
            MSC.WriteLine("Betting round quit. Current players and pots:");

            foreach (BettingPlayer player in round.Players)
            {
                MSC.WriteLine($"Player #{player.Id}: status={player.Status}, remaining={player.RemainingStack}, contribution={player.Contribution}");
            }

            PrintCurrentPots(round);

            if (CanContinueBettingRound(round.Players))
            {
                MSC.WriteLine("A new betting round can start because more than one player still has chips.");
            }
            else
            {
                MSC.WriteLine("A new betting round cannot start because fewer than two players still have chips.");
            }
        }

        void PrintCurrentPots(BettingRound round)
        {
            var levels = round.Players
                .Where(player => player.Contribution > 0)
                .Select(player => player.Contribution)
                .Distinct()
                .OrderBy(level => level)
                .ToList();

            if (levels.Count == 0)
            {
                MSC.WriteLine("Current pots: none");
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

                MSC.WriteLine($"Current pot #{index}: {amount} chips | contributors: {string.Join(", ", contributors.Select(playerId => $"#{playerId}"))} | eligible: {string.Join(", ", eligiblePlayers.Select(playerId => $"#{playerId}"))}");
                previousLevel = level;
            }
        }

        void Bet(ushort players)
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

        void Betting(IReadOnlyList<BettingPlayer> players)
        {
            if (players.Count < 2)
            {
                throw new ArgumentException("Betting requires at least two players.", nameof(players));
            }

            BettingRound round = new(players);

            MSC.WriteLine($"Started a betting round with {players.Count} players, each holding the current stack values.");

            BettingPlayer? playerToAct = round.Next();
            while (playerToAct != null)
            {
                while (true)
                {
                    long currentBiggestBet = round.BiggestContribution;
                    long amountToCall = Math.Max(0, currentBiggestBet - playerToAct.Contribution);

                    MSC.WriteLine($"Player #{playerToAct.Id} has {playerToAct.RemainingStack} chips remaining.");
                    if (currentBiggestBet > 0)
                    {
                        MSC.WriteLine($"Current biggest bet is {currentBiggestBet}. Enter 'CALL' to match it, or enter a bigger total contribution to raise.");
                    }
                    else
                    {
                        MSC.WriteLine("No bet has been placed yet. Enter 'CHECK' to check, or enter any positive amount to open the betting.");
                    }

                    MSC.Write("Enter an amount to contribute, or F to fold: ");
                    string? action = MSC.ReadLine();

                    if (string.IsNullOrWhiteSpace(action))
                    {
                        MSC.WriteLine("Please enter a valid contribution amount or F to fold.");
                        continue;
                    }

                    if (IsQuitCommand(action))
                    {
                        QuitBettingRound(round);
                        return;
                    }

                    if (action.Equals("F", StringComparison.OrdinalIgnoreCase))
                    {
                        round.Fold(playerToAct.Id);
                        MSC.WriteLine($"Player #{playerToAct.Id} folded.");
                        break;
                    }

                    if (action.Equals("CHECK", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            round.Check(playerToAct.Id);
                            MSC.WriteLine($"Player #{playerToAct.Id} checked.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            MSC.WriteLine(ex.Message);
                        }

                        continue;
                    }

                    if (action.Equals("CALL", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            round.Call(playerToAct.Id);
                            MSC.WriteLine($"Player #{playerToAct.Id} called and contributed {amountToCall} chips.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            MSC.WriteLine(ex.Message);
                        }

                        continue;
                    }

                    if (action.Equals("ALLIN", StringComparison.OrdinalIgnoreCase) || action.Equals("ALL-IN", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            long allInAmount = playerToAct.RemainingStack;
                            round.AllIn(playerToAct.Id);
                            MSC.WriteLine($"Player #{playerToAct.Id} went all-in with {allInAmount} chips.");
                            break;
                        }
                        catch (Exception ex)
                        {
                            MSC.WriteLine(ex.Message);
                        }

                        continue;
                    }

                    if (!long.TryParse(action, out long targetContribution) || targetContribution <= 0)
                    {
                        MSC.WriteLine("Contribution must be a positive integer.");
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
                        MSC.WriteLine($"Player #{playerToAct.Id} contributed {contribution} chips and now has {targetContribution} total chips in the pot.");
                        break;
                    }
                    catch (Exception ex)
                    {
                        MSC.WriteLine(ex.Message);
                    }
                }

                playerToAct = round.Next();
            }

            round.Close();
            IReadOnlyList<BettingPot> pots = round.GetPots();

            MSC.WriteLine("Betting round closed. Pots:");
            foreach (BettingPot pot in pots)
            {
                string eligiblePlayers = pot.EligiblePlayers.Count == 0
                    ? "none"
                    : string.Join(", ", pot.EligiblePlayers.Select(playerId => $"#{playerId}"));

                MSC.WriteLine($"Pot #{pot.Index}: {pot.Amount} chips | contributors: {string.Join(", ", pot.Contributors.Select(playerId => $"#{playerId}"))} | eligible: {eligiblePlayers}");
            }

            var winnersByPot = new Dictionary<int, IReadOnlyCollection<ushort>>();
            foreach (BettingPot pot in pots)
            {
                while (true)
                {
                    MSC.WriteLine($"Pot #{pot.Index} is contested by players: {string.Join(", ", pot.EligiblePlayers.Select(playerId => $"#{playerId}"))}.");
                    if (pot.EligiblePlayers.Count == 1)
                    {
                        MSC.WriteLine($"Only one player is eligible for this pot, so Player #{pot.EligiblePlayers[0]} is the winner by default.");
                        winnersByPot[pot.Index] = new ushort[] { pot.EligiblePlayers[0] };
                        break;
                    }
                    MSC.Write("Inform the winning player id(s) for this pot (comma separated, or press Enter to select the first eligible player): ");
                    string? rawWinners = MSC.ReadLine();

                    try
                    {
                        IReadOnlyCollection<ushort> winners;
                        if (string.IsNullOrWhiteSpace(rawWinners))
                        {
                            winners = new ushort[] { pot.EligiblePlayers[0] };
                        }
                        else
                        {
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

                            winners = parsedWinners;
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

            MSC.WriteLine("Settlement payouts:");
            foreach (BettingPayout payout in settlement.Payouts.OrderBy(payout => payout.PotIndex).ThenBy(payout => payout.PlayerId))
            {
                MSC.WriteLine($"Pot #{payout.PotIndex}: Player #{payout.PlayerId} receives {payout.Amount} chips.");
            }

            MSC.WriteLine($"Total pot: {settlement.TotalPot}; total payouts: {settlement.TotalPayout}.");

            IReadOnlyList<BettingPlayer> nextPlayers = settlement.NextPlayers;

            if (nextPlayers.Count < 2)
            {
                MSC.WriteLine("Betting ended because fewer than two players still have chips.");
                return;
            }

            MSC.WriteLine("Starting a new betting round with the current players and chip values.");
            foreach (BettingPlayer player in nextPlayers)
            {
                MSC.WriteLine($"Player #{player.Id} starts with {player.RemainingStack} chips.");
            }

            PrintCurrentPots(round);
            Betting(nextPlayers);
        }

        void TexasHoldem(ushort players)
        {
            TexasHoldemGame game = new(players);

            foreach (KeyValuePair<ushort, TexasHoldemPlayerCards> player in game.PlayersCards)
            {
                MSC.WriteLine($"Player #{player.Key} have [{player.Value.FirstCard}, {player.Value.SecondCard}] in hand");
            }

            MSC.WriteLine("Press any key to continue to the table cards...");
            MSC.ReadLine();

            IReadOnlyList<Card> flop = game.Continue();
            MSC.WriteLine($"Table flop is [{flop[0]}, {flop[1]}, {flop[2]}]");
            MSC.WriteLine("Press any key to continue to the turn card...");
            MSC.ReadLine();

            IReadOnlyList<Card> turn = game.Continue();
            MSC.WriteLine($"Table turn is {turn[3]}");
            MSC.WriteLine("Press any key to continue to the river card...");
            MSC.ReadLine();

            IReadOnlyList<Card> river = game.Continue();
            MSC.WriteLine($"Table river is {river[4]}");
            MSC.WriteLine("Press any key to continue to the showdown...");
            MSC.ReadLine();

            IReadOnlyList<KeyValuePair<ushort, PokerHand>> hands = game.GetBestHands();
            foreach (KeyValuePair<ushort, PokerHand> hand in hands)
            {
                string status = hand.Key == 0 ? "Table" : $"Player #{hand.Key}";
                MSC.WriteLine($"{status} best possible hand is {hand.Value}");
            }

            MSC.WriteLine("Press any key to goes to winner announcement...");
            MSC.ReadLine();
            PrintWinner(hands);
            _deck = new CardDeck();
            _deck.PowerShuffle();
        }

        void SimpleGame(ushort players)
        {
            SimpleCardGame game = new(players);

            game.DealCards();

            foreach (KeyValuePair<ushort, IReadOnlyList<Card>> player in game.PlayersCards)
            {
                string cardsString = $"Player #{player.Key} have [";
                cardsString += string.Join(", ", player.Value.Select(card => card.ToString()));
                cardsString += "]";
                MSC.WriteLine(cardsString);
            }

            MSC.WriteLine("Final hands:");
            foreach (KeyValuePair<ushort, PokerHand> player in game.GetRankedHands())
            {
                MSC.WriteLine($"Player #{player.Key} has {player.Value.ToString()}");
            }

            MSC.WriteLine($"Winner is Player #{game.GetWinnerPlayer()}");
        }

        void PrintWinner(IReadOnlyList<KeyValuePair<ushort, PokerHand>> playersHands)
        {
            ushort win = playersHands.FirstOrDefault().Key;
            MSC.WriteLine(win == 0 ? "The table winner" : $"The winner is player #{win}".ToUpperInvariant());
            MSC.WriteLine($"Ranked players hands:");
            foreach (KeyValuePair<ushort, PokerHand> item in playersHands)
            {
                MSC.WriteLine(item.Key == 0 ? $"Table has {item.Value}" : $"Player #{item.Key} has {item.Value}");
            }
        }
    }
}