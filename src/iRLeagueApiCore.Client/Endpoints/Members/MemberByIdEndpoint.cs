using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Members;
internal sealed class MemberByIdEndpoint : UpdateEndpoint<MemberModel, PutMemberModel>, IMemberByIdEndpoint
{
    public MemberByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long id) : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddParameter(id);
    }
}
