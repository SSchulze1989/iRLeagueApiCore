using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints.Results;
internal sealed class ResultRowsEndpoint : EndpointBase, IResultRowsEndpoint
{
    public ResultRowsEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) 
        : base(httpClient, routeBuilder, "Rows")
    {
    }

    public IResultRowByIdEndpoint WithId(long id)
    {
        return new ResultRowByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
