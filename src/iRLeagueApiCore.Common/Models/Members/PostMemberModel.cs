namespace iRLeagueApiCore.Common.Models;

[DataContract]
public class PostMemberModel
{
    [DataMember]
    public string IRacingId { get; set; } = string.Empty;
}
