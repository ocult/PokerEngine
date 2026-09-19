using PokerEngine.Domain.Models;
using PokerEngine.Domain.OmahaHoldem;

public class OmahaHoldemFactory
{
    static Dictionary<Guid, OmahaHoldemGame> _games = new();

    public static OmahaHoldemGame CreateOmahaHoldemGame(ushort players, out Guid id)
    {
        id = Guid.NewGuid();
        _games.Add(id, new(players));
        return _games[id];
    }

    internal static OmahaHoldemGame? GetGameById(Guid gameId)
    {
        return _games.TryGetValue(gameId, out OmahaHoldemGame? game) ? game : null;
    }
}