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

        public SeasonByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder)
        {
            this.httpClient = httpClient;
            this.routeBuilder = routeBuilder;
        }

        Task<ClientActionResult<NoContent>> ISeasonByIdEndpoint.Delete(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        Task<ClientActionResult<GetSeasonModel>> ISeasonByIdEndpoint.Get(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        Task<ClientActionResult<GetSeasonModel>> ISeasonByIdEndpoint.Put(PutSeasonModel model, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}