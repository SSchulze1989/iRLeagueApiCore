namespace iRLeagueApiCore.Common.Models.Rosters;

[DataContract]
public sealed class RosterEntryInfoModel
{
    [DataMember]
    public long MemberId { get; set; }
    [DataMember]
    public string Firstname { get; set; } = string.Empty;
    [DataMember]
    public string Lastname { get; set; } = string.Empty;

    [DataMember]
    public long? TeamId { get; set; }
    [DataMember]
    public string TeamName {  get; set; } = string.Empty;
    [DataMember]
    public string TeamColor {  get; set; } = string.Empty;

    [DataMember]
    public Dictionary<string, string> Profile { get; set; } = [];
}
