using PokerEngine.Domain.Models;

public class TexasHoldemFactory
{
    static Dictionary<Guid, TexasHoldemGame> _games = new();

    public static TexasHoldemGame CreateTexasHoldemGame(ushort players, out Guid id)
    {
        id = Guid.NewGuid();
        _games.Add(id, new(players));
        return _games[id];
    }

    internal static TexasHoldemGame? GetGameById(Guid gameId)
    {
        return _games.TryGetValue(gameId, out TexasHoldemGame? game) ? game : null;
    }
}