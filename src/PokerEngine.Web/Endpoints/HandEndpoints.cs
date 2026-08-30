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
            return Results.BadRequest(new { error = "Informe 5 cartas no formato AC, KC, QH, JD, TS." });
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
            throw new ArgumentException("É necessário informar exatamente 5 cartas.");
        }

        return string.Join(", ", values.Select(NormalizeCardToken));
    }

    private static string NormalizeCardToken(string value)
    {
        string normalized = value.Trim();
        if (normalized.Length == 3 && normalized.StartsWith("10", StringComparison.OrdinalIgnoreCase))
        {
            return $"T{normalized[2]}".ToUpperInvariant();
        }

        return normalized.ToUpperInvariant();
    }
}
