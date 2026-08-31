namespace iRLeagueApiCore.Server.Models;

internal static class CacheKeys
{
    public static string GetLeagueNameKey(string leagueName) => $"leagueName_{leagueName}";
    public static string IracingOauthTokenKey => "iracing_oauth_token";

    public static string GetResultsByEventKey(long eventId) => $"results:event:{eventId}";
    public static string GetResultsBySeasonKey(long seasonId) => $"results:season:{seasonId}";
    public static string GetStandingsByEventKey(long eventId) => $"standings:event:{eventId}";
    public static string GetStandingsBySeasonKey(long seasonId) => $"standings:season:{seasonId}";

    public static readonly TimeSpan ResultsCacheDuration = TimeSpan.FromMinutes(30);
    public static readonly TimeSpan StandingsCacheDuration = TimeSpan.FromMinutes(30);
}
