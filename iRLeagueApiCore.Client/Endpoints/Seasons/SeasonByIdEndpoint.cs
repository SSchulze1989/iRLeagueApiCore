using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    internal class SeasonByIdEndpoint : UpdateEndpoint<GetSeasonModel, PutSeasonModel>, ISeasonByIdEndpoint
    {

        public SeasonByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long seasonId) :
            base(httpClient, routeBuilder)
        {
            routeBuilder.AddParameter(seasonId);
        }
    }
}