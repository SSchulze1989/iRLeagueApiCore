using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Members;

public interface IMembersEndpoint : IGetAllEndpoint<MemberModel>, IWithIdEndpoint<IMemberByIdEndpoint>
{
    public IPostEndpoint<MemberModel> FetchProfileFromIracing(string iracingId);
    public IPostEndpoint<MemberModel> AddMemberFromIracing(string iracingId);
}
