namespace iRLeagueApiCore.Client.ResultsParsing;

public sealed class ParseTrack
{
    public int track_id { get; set; }
    public string? track_name { get; set; }
    public string? config_name { get; set; }
    public int category_id { get; set; }
    public string? category { get; set; }
}
