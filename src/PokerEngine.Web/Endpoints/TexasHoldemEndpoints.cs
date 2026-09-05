using PokerEngine.Domain.Models;

namespace PokerEngine.Web.Endpoints;

public static class TexasHoldemEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/texas-holdem", GetTexasHoldemByQueryString);
        app.MapPost("/api/texas-holdem", PostTexasHoldemFromRequest);
    }

    public static IResult GetTexasHoldemByQueryString(ushort players)
    {
        return CreateTexasHoldemGame(players);
    }

    public static async Task<IResult> PostTexasHoldemFromRequest(HttpRequest request)
    {
        string? players = request.HasFormContentType ? request.Form["players"].ToString() : null;
        string? gameId = request.HasFormContentType ? request.Form["gameid"].ToString() : null;
        
        if (string.IsNullOrWhiteSpace(players) && string.IsNullOrWhiteSpace(gameId))
        {
            var payload = await request.ReadFromJsonAsync<TexasHoldemRequest>();
            players = payload?.Players.ToString();
            gameId = payload?.GameId.ToString();
        }

        if (string.IsNullOrWhiteSpace(gameId))
        {
            return ushort.TryParse(players, out ushort parsedPlayers)
                ? CreateTexasHoldemGame(parsedPlayers)
                : Results.BadRequest(new { error = "You need to input a valid player number." });
        }

        return Guid.TryParse(gameId, out Guid parsedGameId)
            ? ContinueTexasHoldemGame(parsedGameId)
            : Results.BadRequest(new { error = "You need to input a valid game ID, or informe a player number to new game." });
    }

    private static IResult CreateTexasHoldemGame(ushort players)
    {
        if (players is 0 or > 21)
        {
            return Results.BadRequest(new { error = "You need to input a valid number of players between 1 and 21." });
        }

        TexasHoldemGame game = TexasHoldemFactory.CreateTexasHoldemGame(players, out Guid id);

        return Results.Ok(new
        {
            players,
            gameId = id,
            phase = game.Stage.ToString(),
            holeCards = game.PlayersCards.ToDictionary(
                item => item.Key,
                item => new[] { item.Value.FirstCard.ToString(), item.Value.SecondCard.ToString() }),
            communityCards = game.CommunityCards.Select(card => card.ToString()).ToArray(),
            bestHands = default(IEnumerable<KeyValuePair<ushort, PokerHand>>),
            winner = default(KeyValuePair<ushort, PokerHand>)
        });
    }

    private static IResult ContinueTexasHoldemGame(Guid gameId)
    {
        TexasHoldemGame? game = TexasHoldemFactory.GetGameById(gameId);

        if (game is null)
        {
            return Results.NotFound(new { error = "Game not found." });
        }

        if (game.Stage != TexasHoldemStage.Complete) game.Continue();

        IReadOnlyList<KeyValuePair<ushort, PokerHand>>? bestHands = game.Stage != TexasHoldemStage.PreFlop ? game.GetBestHands() : default;
        KeyValuePair<ushort, PokerHand> winner = game.Stage == TexasHoldemStage.Complete ? bestHands.First() : default;

        return Results.Ok(new
        {
            players = game.Players,
            gameId,
            phase = game.Stage.ToString(),
            holeCards = game.PlayersCards.ToDictionary(
                item => item.Key,
                item => new[] { item.Value.FirstCard.ToString(), item.Value.SecondCard.ToString() }),
            communityCards = game.CommunityCards.Select(card => card.ToString()).ToArray(),
            bestHands = bestHands is null ? null : bestHands.Select(item => new
            {
                player = item.Key,
                ranking = item.Value.HandRanking.ToString(),
                description = item.Value.ToString()
            }).ToArray(),
            winner = new
            {
                player = winner.Key,
                ranking = winner.Value.HandRanking.ToString(),
                description = winner.Value.ToString()
            }
        });
    }
}
