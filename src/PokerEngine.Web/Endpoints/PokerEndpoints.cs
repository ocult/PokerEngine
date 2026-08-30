using PokerEngine.Domain.Models;

namespace PokerEngine.Web.Endpoints;

public static class PokerEndpoints
{
    public static void MapPokerEndpoints(this WebApplication app)
    {
        HandEndpoints.Map(app);
        TexasHoldemEndpoints.Map(app);
    }
}
