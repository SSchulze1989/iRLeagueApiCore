using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    internal class SessionByIdEndpoint : UpdateEndpoint<GetSessionModel, PutSessionModel>, ISessionByIdEndpoint
    {
        public SessionByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long sessionId) : 
            base(httpClient, routeBuilder)
        {
            routeBuilder.AddParameter(sessionId);
        }

        IResultsEndpoint ISessionByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClient, RouteBuilder);
        }
    }
}