using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Members;

internal sealed class MembersEndpoint : GetAllEndpoint<MemberModel>, IMembersEndpoint
{
    public MembersEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Members");
    }

    IMemberByIdEndpoint IWithIdEndpoint<IMemberByIdEndpoint, long>.WithId(long id)
    {
        return new MemberByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }

    IPostEndpoint<MemberModel> IMembersEndpoint.FetchProfileFromIracing(string iracingId)
    {
        return new FetchProfileEndpoint(HttpClientWrapper, RouteBuilder, iracingId.ToString());
    }

    IPostEndpoint<MemberModel> IMembersEndpoint.AddMemberFromIracing(string iracingId)
    {
        return new AddProfileEndpoint(HttpClientWrapper, RouteBuilder, iracingId.ToString());
    }
}
