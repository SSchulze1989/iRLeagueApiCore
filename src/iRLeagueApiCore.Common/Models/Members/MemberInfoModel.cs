
namespace iRLeagueApiCore.Common.Models;

[DataContract]
public class MemberInfoModel : IEquatable<MemberInfoModel>
{
    [DataMember]
    public long MemberId { get; set; }
    [DataMember]
    public string FirstName { get; set; } = string.Empty;
    [DataMember]
    public string LastName { get; set; } = string.Empty;

    bool IEquatable<MemberInfoModel>.Equals(MemberInfoModel? other)
    {
        return MemberId == other?.MemberId;
    }

    public static implicit operator MemberInfoModel(MemberModel m) => new()
    {
        MemberId = m.MemberId,
        FirstName = m.Firstname,
        LastName = m.Lastname,
    };
}
