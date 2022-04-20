using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    internal class SeasonByIdEndpoint : ISeasonByIdEndpoint
    {
        private readonly HttpClient httpClient;
        private readonly RouteBuilder routeBuilder;

        private string QueryUrl => routeBuilder.Build();

        public SeasonByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long seasonId)
        {
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
            routeBuilder.AddParameter(seasonId);
        }

        async Task<ClientActionResult<NoContent>> ISeasonByIdEndpoint.Delete(CancellationToken cancellationToken)
        {
            return await httpClient.DeleteAsClientActionResult(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<GetSeasonModel>> ISeasonByIdEndpoint.Get(CancellationToken cancellationToken)
        {
            return await httpClient.GetAsClientActionResult<GetSeasonModel>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<GetSeasonModel>> ISeasonByIdEndpoint.Put(PutSeasonModel model, CancellationToken cancellationToken)
        {
            return await httpClient.PutAsClientActionResult<GetSeasonModel, PutSeasonModel>(QueryUrl, model, cancellationToken);
        }
    }
}