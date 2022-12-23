using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models.Members;

namespace iRLeagueApiCore.Client.Endpoints.Members;

public class MembersEndpoint : GetAllEndpoint<MemberInfoModel>, IMembersEndpoint
{
    public MembersEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Members");
    }
}
