using PokerEngine.Domain.Betting;
using PokerEngine.Domain.Holdem;
using PokerEngine.Domain.Models;
using PokerEngine.Domain.TexasHoldem;
using MSC = System.Console;

namespace PokerEngine.Console
{
    public static class TexasRunner
    {
        public static void Help()
        {
            MSC.WriteLine("TEXAS: 'texas [players]' - Plays Texas Hold'em dealing hands, flop, turn, river, and showdown for [players] (2+).");
        }

        public static void Run(ushort players, IReadOnlyList<BettingPlayer>? currentPlayers = null)
        {
            TexasHoldemGame game = new(players);
            HoldemTable<TexasHoldemGame, TexasHoldemPlayerCards> table = new(game);
            IReadOnlyList<BettingPlayer> activePlayers = currentPlayers is not null && currentPlayers.Count == players
                ? currentPlayers
                : Enumerable.Range(1, players)
                    .Select(playerNumber => new BettingPlayer((ushort)playerNumber, 100))
                    .ToList();

            Func<ushort, string> getPlayerCards = playerId =>
                $"[{game.PlayersCards[playerId].FirstCard}, {game.PlayersCards[playerId].SecondCard}]";

            Func<ushort, string> getBestHandText = playerId =>
                game.CommunityCards.Count >= 3
                    ? game.GetBestHands()
                        .FirstOrDefault(hand => hand.Key == playerId)
                        .Key == playerId
                            ? game.GetBestHands().First(hand => hand.Key == playerId).Value.ToString()
                            : "No hand available"
                    : string.Empty;

            try
            {
                activePlayers = HoldemRunner.PlayPreFlop(table, activePlayers, getPlayerCards, getBestHandText);

                activePlayers = HoldemRunner.PlayStreet(
                    table,
                    activePlayers,
                    "flop",
                    () => game.CommunityCards,
                    flop => MSC.WriteLine($"Table cards: [{string.Join(", ", flop)}]"),
                    getPlayerCards,
                    getBestHandText);

                activePlayers = HoldemRunner.PlayStreet(
                    table,
                    activePlayers,
                    "turn",
                    () => game.CommunityCards,
                    turn => MSC.WriteLine($"Table cards: [{string.Join(", ", turn)}]"),
                    getPlayerCards,
                    getBestHandText);

                activePlayers = HoldemRunner.PlayStreet(
                    table,
                    activePlayers,
                    "river",
                    () => game.CommunityCards,
                    river => MSC.WriteLine($"Table cards: [{string.Join(", ", river)}]"),
                    getPlayerCards,
                    getBestHandText);
            }
            catch (ConsoleRoundQuitException)
            {
                MSC.WriteLine("Quit requested. Exiting the Texas Hold'em round.");
                return;
            }

            HoldemRunner.CompleteHand(table);

            IReadOnlyList<BettingPlayer> remainingPlayers = activePlayers
                .Where(player => player.Status != BettingPlayerStatus.Folded)
                .ToList();

            IReadOnlyList<KeyValuePair<ushort, PokerHand>> hands = game.GetBestHands();

            if (remainingPlayers.Count <= 1)
            {
                MSC.WriteLine("Press any key to go to the pot winner announcement...");
                string? input = MSC.ReadLine();
                if (Program.IsQuitCommand(input))
                {
                    return;
                }

                HoldemRunner.PrintPotWinner(
                    remainingPlayers,
                    playerId => getPlayerCards(playerId),
                    game.CommunityCards,
                    remainingPlayers.Count == 1 ? new ushort[] { remainingPlayers[0].Id } : null);
                activePlayers = HoldemRunner.SettleShowdown(table, activePlayers, hands);
                HoldemRunner.PrintPlayerStacks(activePlayers);
                if (activePlayers.Count(player => player.RemainingStack > 0) > 1)
                {
                    IReadOnlyList<BettingPlayer> nextRoundPlayers = table.PrepareNextRoundPlayers(activePlayers);
                    MSC.WriteLine("Starting next round with the current chip values.");
                    Run(players, nextRoundPlayers);
                }
                return;
            }
            foreach (KeyValuePair<ushort, PokerHand> hand in hands)
            {
                string status = hand.Key == 0 ? "Table" : $"Player #{hand.Key}";
                MSC.WriteLine($"{status} best possible hand is {hand.Value}");
            }

            HoldemRunner.PrintCurrentPots(activePlayers);

            MSC.WriteLine("Press any key to goes to winner announcement...");
            string? continueInput = MSC.ReadLine();
            if (Program.IsQuitCommand(continueInput))
            {
                return;
            }

            HoldemRunner.PrintPotWinner(
                remainingPlayers,
                playerId => getPlayerCards(playerId),
                game.CommunityCards,
                hands
                    .Where(hand => hand.Value.Equals(hands.First().Value))
                    .Select(hand => hand.Key)
                    .ToArray());
            HoldemRunner.PrintWinner(hands, playerId => getPlayerCards(playerId), game.CommunityCards);
            activePlayers = HoldemRunner.SettleShowdown(table, activePlayers, hands);
            HoldemRunner.PrintPlayerStacks(activePlayers);

            if (activePlayers.Count(player => player.RemainingStack > 0) > 1)
            {
                IReadOnlyList<BettingPlayer> nextRoundPlayers = table.PrepareNextRoundPlayers(activePlayers);
                MSC.WriteLine("Starting next round with the current chip values.");
                Run(players, nextRoundPlayers);
            }
        }
    }
}
