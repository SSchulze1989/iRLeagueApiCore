namespace iRLeagueApiCore.Client.ResultsParsing;

public sealed class ParseTrackState
{
    public bool leave_marbles { get; set; }
    public int practice_rubber { get; set; }
    public int qualify_rubber { get; set; }
    public int warmup_rubber { get; set; }
    public int race_rubber { get; set; }
    public int practice_grip_compound { get; set; }
    public int qualify_grip_compound { get; set; }
    public int warmup_grip_compound { get; set; }
    public int race_grip_compound { get; set; }
}
