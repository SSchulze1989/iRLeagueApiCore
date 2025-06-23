namespace iRLeagueApiCore.Common.Models.Rosters;

[DataContract]
public class RosterEntryModel
{ 
    [DataMember]
    public long MemberId { get; set; }
    [DataMember]
    public long? TeamId { get; set; }
    [DataMember]
    public string Number { get; set; } = string.Empty;
    [DataMember]
    public Dictionary<string, string> Profile { get; set; } = [];
}
