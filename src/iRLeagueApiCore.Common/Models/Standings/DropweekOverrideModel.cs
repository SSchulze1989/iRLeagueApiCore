using iRLeagueApiCore.Common.Models.Tracks;

namespace iRLeagueApiCore.Common.Models;

[DataContract]
public sealed class DropweekOverrideModel : PutDropweekOverrideModel
{
    [DataMember]
    public long StandingConfigId { get; set; }
    [DataMember]
    public long ScoredResultRowId { get; set; }
    [DataMember]
    public MemberInfoModel? Member { get; set; }
    [DataMember]
    public TeamInfoModel? Team { get; set; }
    [DataMember]
    public string EventName { get; set; } = string.Empty;
    [DataMember]
    public DateTime Date { get; set; }
    [DataMember]
    public long TrackId { get; set; }
    [DataMember]
    public string TrackName { get; set; } = string.Empty;
    [DataMember]
    public string ConfigName { get; set; } = string.Empty;
}