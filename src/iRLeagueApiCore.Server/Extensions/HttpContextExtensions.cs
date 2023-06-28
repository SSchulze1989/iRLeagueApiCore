using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Extensions;

internal static class HttpContextExtensions
{
    public static LeagueUser? GetLeagueUser(this HttpContext context)
    {
        var leagueNameObject = context.GetRouteValue("leagueName");
        if (leagueNameObject is null) 
        { 
            return null;
        }
        return new LeagueUser((string)leagueNameObject, context.User);
    }
}
