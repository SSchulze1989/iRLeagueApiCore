namespace iRLeagueApiCore.Common.Models;

[DataContract]
public sealed class MemberModel : MemberInfoModel
{
    [DataMember]
    public string IRacingId { get; set; } = string.Empty;
    [DataMember]
    public string TeamName { get; set; } = string.Empty;
    [DataMember]
    public string Number {  get; set; } = string.Empty;
    [DataMember]
    public string DiscordId { get; set; } = string.Empty;
}
