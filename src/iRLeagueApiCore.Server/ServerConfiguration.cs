namespace iRLeagueApiCore.Server;

public sealed class ServerConfiguration
{
    public int MaxLeaguesPerUser { get; set; } = 3;

    public int TriggerCheckIntervalMs { get; set; } = 60000;
}
