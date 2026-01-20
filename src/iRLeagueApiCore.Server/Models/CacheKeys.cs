namespace iRLeagueApiCore.Server.Models;

internal static class CacheKeys
{
    public static string GetLeagueNameKey(string leagueName) => $"leagueName_{leagueName}";
    public static string IracingOauthTokenKey => "iracing_oauth_token";
}
