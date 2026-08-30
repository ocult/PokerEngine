using PokerEngine.Domain.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => Results.Content(GetHomePage(), "text/html"));

app.MapGet("/api/hand", (string? cards) => EvaluateHand(cards));
app.MapPost("/api/hand", async (HttpRequest request) =>
{
    string? cards = request.HasFormContentType ? request.Form["cards"].ToString() : null;
    if (string.IsNullOrWhiteSpace(cards))
    {
        cards = await request.ReadFromJsonAsync<HandRequest>() is { Cards: { Length: > 0 } } payload ? payload.Cards : null;
    }

    return EvaluateHand(cards);
});

app.MapGet("/api/texas-holdem", (ushort players) => EvaluateTexasHoldem(players));
app.MapPost("/api/texas-holdem", async (HttpRequest request) =>
{
    string? players = request.HasFormContentType ? request.Form["players"].ToString() : null;
    if (string.IsNullOrWhiteSpace(players))
    {
        var payload = await request.ReadFromJsonAsync<TexasHoldemRequest>();
        players = payload?.Players.ToString();
    }

    return ushort.TryParse(players, out ushort parsedPlayers)
        ? EvaluateTexasHoldem(parsedPlayers)
        : Results.BadRequest(new { error = "Informe um número de jogadores válido." });
});

app.Run();

static IResult EvaluateHand(string? cards)
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

static string NormalizeCards(string cards)
{
    string[] values = cards.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
    if (values.Length != 5)
    {
        throw new ArgumentException("É necessário informar exatamente 5 cartas.");
    }

    return string.Join(", ", values.Select(NormalizeCardToken));
}

static string NormalizeCardToken(string value)
{
    string normalized = value.Trim();
    if (normalized.Length == 3 && normalized.StartsWith("10", StringComparison.OrdinalIgnoreCase))
    {
        return $"T{normalized[2]}".ToUpperInvariant();
    }

    return normalized.ToUpperInvariant();
}

static IResult EvaluateTexasHoldem(ushort players)
{
    if (players is 0 or > 9)
    {
        return Results.BadRequest(new { error = "Informe um número de jogadores entre 1 e 9." });
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
        flop = flop.Select(card => card.ToString()).ToArray(),
        turn = turn.Select(card => card.ToString()).ToArray(),
        river = river.Select(card => card.ToString()).ToArray(),
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

static string GetHomePage()
{
    return """
    <!doctype html>
    <html lang=\"pt-BR\">
    <head>
        <meta charset=\"utf-8\" />
        <title>PokerEngine Web</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 900px; margin: 40px auto; padding: 0 20px; }
            .card { margin-bottom: 20px; } 
            input, button { padding: 10px; font-size: 1rem; }
            input { width: 260px; }
            button { margin-left: 10px; }
            pre { background: #f3f3f3; padding: 16px; border-radius: 8px; overflow: auto; }
        </style>
    </head>
    <body>
        <h1>PokerEngine Web</h1>

        <div class=\"card\">
            <h2>Avaliar mão</h2>
            <form action=\"/api/hand\" method=\"post\">
                <input name=\"cards\" value=\"AC, KC, QH, JD, 10S\" />
                <button type=\"submit\">Avaliar</button>
            </form>
        </div>

        <div class=\"card\">
            <h2>Texas Hold'em</h2>
            <form action=\"/api/texas-holdem\" method=\"post\">
                <input name=\"players\" value=\"2\" type=\"number\" min=\"1\" max=\"9\" />
                <button type=\"submit\">Executar</button>
            </form>
        </div>
    </body>
    </html>
    """;
}

public record HandRequest(string? Cards);
public record TexasHoldemRequest(ushort Players);
