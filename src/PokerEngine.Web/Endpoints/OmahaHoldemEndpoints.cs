using PokerEngine.Domain.Models;
using PokerEngine.Domain.OmahaHoldem;

namespace PokerEngine.Web.Endpoints;

public record OmahaHoldemRequest(ushort Players, Guid GameId);

public static class OmahaHoldemEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/omaha-holdem", GetOmahaHoldemByQueryString);
        app.MapPost("/api/omaha-holdem", PostOmahaHoldemFromRequest);
    }

    public static IResult GetOmahaHoldemByQueryString(ushort players)
    {
        return CreateOmahaHoldemGame(players);
    }

    public static async Task<IResult> PostOmahaHoldemFromRequest(HttpRequest request)
    {
        string? players = request.HasFormContentType ? request.Form["players"].ToString() : null;
        string? gameId = request.HasFormContentType ? request.Form["gameid"].ToString() : null;
        
        if (string.IsNullOrWhiteSpace(players) && string.IsNullOrWhiteSpace(gameId))
        {
            var payload = await request.ReadFromJsonAsync<OmahaHoldemRequest>();
            players = payload?.Players.ToString();
            gameId = payload?.GameId.ToString();
        }

        if (string.IsNullOrWhiteSpace(gameId))
        {
            return ushort.TryParse(players, out ushort parsedPlayers)
                ? CreateOmahaHoldemGame(parsedPlayers)
                : Results.BadRequest(new { error = "You need to input a valid player number." });
        }

        return Guid.TryParse(gameId, out Guid parsedGameId)
            ? ContinueOmahaHoldemGame(parsedGameId)
            : Results.BadRequest(new { error = "You need to input a valid game ID, or informe a player number to new game." });
    }

    private static IResult CreateOmahaHoldemGame(ushort players)
    {
        if (players is 0 or > 10)
        {
            return Results.BadRequest(new { error = "You need to input a valid number of players between 1 and 10." });
        }

        OmahaHoldemGame game = OmahaHoldemFactory.CreateOmahaHoldemGame(players, out Guid id);

        return Results.Ok(new
        {
            players,
            gameId = id,
            phase = game.Stage.ToString(),
            holeCards = game.PlayersCards.ToDictionary(
                item => item.Key,
                item => new[] 
                { 
                    item.Value.FirstCard.ToString(), 
                    item.Value.SecondCard.ToString(), 
                    item.Value.ThirdCard.ToString(), 
                    item.Value.FourthCard.ToString()
                }),
            communityCards = game.CommunityCards.Select(card => card.ToString()).ToArray(),
            bestHands = default(IEnumerable<KeyValuePair<ushort, PokerHand>>),
            winner = default(KeyValuePair<ushort, PokerHand>)
        });
    }

    private static IResult ContinueOmahaHoldemGame(Guid gameId)
    {
        OmahaHoldemGame? game = OmahaHoldemFactory.GetGameById(gameId);

        if (game is null)
        {
            return Results.NotFound(new { error = "Game not found." });
        }

        if (game.Stage != HoldemStage.Complete) game.Continue();

        IReadOnlyList<KeyValuePair<ushort, PokerHand>>? bestHands = game.Stage != HoldemStage.PreFlop ? game.GetBestHands() : default;
        KeyValuePair<ushort, PokerHand> winner = game.Stage == HoldemStage.Complete ? bestHands.First() : default;

        return Results.Ok(new
        {
            players = game.Players,
            gameId,
            phase = game.Stage.ToString(),
            holeCards = game.PlayersCards.ToDictionary(
                item => item.Key,
                item => new[] 
                { 
                    item.Value.FirstCard.ToString(), 
                    item.Value.SecondCard.ToString(), 
                    item.Value.ThirdCard.ToString(), 
                    item.Value.FourthCard.ToString()
                }),
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
