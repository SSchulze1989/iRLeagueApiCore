using iRLeagueApiCore.Client.Endpoints.Bonuses;
using iRLeagueApiCore.Client.Endpoints.Penalties;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Results;
internal sealed class ResultRowByIdEndpoint : EndpointBase, IResultRowByIdEndpoint
{
    public ResultRowByIdEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder, long id) 
        : base(httpClient, routeBuilder)
    {
        RouteBuilder.AddParameter(id);
    }

    public IPostEndpoint<AddBonusModel, PostAddBonusModel> Bonuses()
    {
        return new BonusesEndpoint(HttpClientWrapper, RouteBuilder);
    }

    public IPostEndpoint<PenaltyModel, PostPenaltyModel> Penalties()
    {
        return new PenaltiesEndpoint(HttpClientWrapper, RouteBuilder);
    }
}
