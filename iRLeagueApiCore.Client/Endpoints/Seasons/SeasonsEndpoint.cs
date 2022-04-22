using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    internal class SeasonsEndpoint : EndpointBase, ISeasonsEndpoint
    {
        public SeasonsEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) :
            base(httpClient, routeBuilder)
        {
            routeBuilder.AddEndpoint("Seasons");
        }

        async Task<ClientActionResult<GetSeasonModel>> IPostEndpoint<GetSeasonModel, PostSeasonModel>.Post(PostSeasonModel model, CancellationToken cancellationToken)
        {
            return await HttpClient.PostAsClientActionResult<GetSeasonModel, PostSeasonModel>(QueryUrl, model, cancellationToken);
        }

        ISeasonByIdEndpoint ISeasonsEndpoint.WitId(long seasonId)
        {
            return new SeasonByIdEndpoint(HttpClient, RouteBuilder.Copy(), seasonId);
        }
    }
}
