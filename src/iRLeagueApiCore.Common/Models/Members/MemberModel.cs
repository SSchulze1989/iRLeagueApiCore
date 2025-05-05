namespace iRLeagueApiCore.Common.Models;

[DataContract]
public sealed class MemberModel : PutMemberModel
{
    [DataMember]
    public long MemberId { get; set; }
    [DataMember]
    public string IRacingId { get; set; } = string.Empty;
    [DataMember]
    public string Firstname { get; set; } = string.Empty;
    [DataMember]
    public string Lastname { get; set; } = string.Empty;
    [DataMember]
    public string TeamName { get; set; } = string.Empty;
}
