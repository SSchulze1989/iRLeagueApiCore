using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Members;
internal class FetchProfileEndpoint : PostEndpoint<MemberModel>
{
    public FetchProfileEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, string iracingId)
        : base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Iracing");
        RouteBuilder.AddEndpoint("Fetch");
        RouteBuilder.WithParameters(x => x.Add("iracing_id", iracingId));
    }
}
