using PokerEngine.Domain.Models;

namespace PokerEngine.Web.Endpoints;

public static class HandEndpoints
{
    public static void Map(WebApplication app)
    {
        app.MapGet("/api/hand", GetHandByQueryString);
        app.MapPost("/api/hand", PostHandFromRequest);
    }

    public static IResult GetHandByQueryString(string? cards)
    {
        return EvaluateHand(cards);
    }

    public static async Task<IResult> PostHandFromRequest(HttpRequest request)
    {
        string? cards = request.HasFormContentType ? request.Form["cards"].ToString() : null;
        if (string.IsNullOrWhiteSpace(cards))
        {
            cards = await request.ReadFromJsonAsync<HandRequest>() is { Cards: { Length: > 0 } } payload ? payload.Cards : null;
        }

        return EvaluateHand(cards);
    }

    private static IResult EvaluateHand(string? cards)
    {
        if (string.IsNullOrWhiteSpace(cards))
        {
            return Results.BadRequest(new { error = "Input 5 cards. Use the format AC, KC, QH, JD, TS." });
        }

        try
        {
            string normalizedCards = NormalizeCards(cards);
            PokerHand hand = new(normalizedCards);
            return Results.Ok(new
            {
                cards = normalizedCards,
                ranking = hand.HandRanking.ToString(),
                description = hand.ToString()
            });
        }
        catch (Exception ex)
        {
            return Results.BadRequest(new { error = ex.Message });
        }
    }

    private static string NormalizeCards(string cards)
    {
        string[] values = cards.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
        if (values.Length != 5)
        {
            throw new ArgumentException("You need to input exactly 5 cards.");
        }

        return string.Join(", ", values.Select(card => card.ToUpperInvariant()));
    }
}
