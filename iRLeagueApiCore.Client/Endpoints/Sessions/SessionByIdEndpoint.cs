using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    internal class SessionByIdEndpoint : UpdateEndpoint<SessionModel, PutSessionModel>, ISessionByIdEndpoint
    {
        public SessionByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long sessionId) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddParameter(sessionId);
        }

        IResultsEndpoint ISessionByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}