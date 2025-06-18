using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Members;
public interface IMemberByIdEndpoint : IGetEndpoint<MemberModel>, IPutEndpoint<MemberModel, PutMemberModel>
{
}
