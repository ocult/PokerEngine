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
        return EvaluateTexasHoldem(players);
    }

    public static async Task<IResult> PostTexasHoldemFromRequest(HttpRequest request)
    {
        string? players = request.HasFormContentType ? request.Form["players"].ToString() : null;
        if (string.IsNullOrWhiteSpace(players))
        {
            var payload = await request.ReadFromJsonAsync<TexasHoldemRequest>();
            players = payload?.Players.ToString();
        }

        return ushort.TryParse(players, out ushort parsedPlayers)
            ? EvaluateTexasHoldem(parsedPlayers)
            : Results.BadRequest(new { error = "You need to input a valid player number." });
    }

    private static IResult EvaluateTexasHoldem(ushort players)
    {
        if (players is 0 or > 21)
        {
            return Results.BadRequest(new { error = "You need to input a valid number of players between 1 and 21." });
        }

        TexasHoldemGame game = new(players);
        IReadOnlyList<Card> flop = game.Continue();
        IReadOnlyList<Card> turn = game.Continue();
        IReadOnlyList<Card> river = game.Continue();

        IReadOnlyList<KeyValuePair<ushort, PokerHand>> bestHands = game.GetBestHands();
        KeyValuePair<ushort, PokerHand> winner = bestHands.First();

        return Results.Ok(new
        {
            players,
            phase = game.Stage.ToString(),
            holeCards = game.PlayersCards.ToDictionary(
                item => item.Key,
                item => new[] { item.Value.FirstCard.ToString(), item.Value.SecondCard.ToString() }),
            communityCards = game.CommunityCards.Select(card => card.ToString()).ToArray(),
            bestHands = bestHands.Select(item => new
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
