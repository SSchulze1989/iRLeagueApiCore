namespace iRLeagueApiCore.Common.Models;

[DataContract]
public class PutMemberModel
{
    [DataMember]
    public long? TeamId { get; set; }
    [DataMember]
    public string Number { get; set; } = string.Empty;
    [DataMember]
    public string DiscordId { get; set; } = string.Empty;
    [DataMember]
    public string CountryFlag { get; set; } = string.Empty;
    [DataMember]
    public Dictionary<string, string> Profile { get; set; } = [];
}
