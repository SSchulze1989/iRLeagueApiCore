using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Standings;
internal sealed class StandingResultRowsEndpoint : EndpointBase, IWithIdEndpoint<IStandingResultRowByIdEndpoint>
{
    public StandingResultRowsEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) 
        : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddEndpoint("ResultRows");
    }

    public IStandingResultRowByIdEndpoint WithId(long id)
    {
        return new StandingResultRowByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }
}
