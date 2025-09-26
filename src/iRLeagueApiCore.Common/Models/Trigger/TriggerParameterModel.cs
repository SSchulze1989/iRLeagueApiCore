namespace iRLeagueApiCore.Common.Models;
public sealed class TriggerParameterModel
{
    public DateTimeOffset? TimeElapesd { get; set; }
    public long? SeasonId { get; set; }
    public long? EventId { get; set; }
    public long? ReviewId { get; set; }
    public long? TeamId { get; set; }
    public long? MemberId { get; set; }
}
