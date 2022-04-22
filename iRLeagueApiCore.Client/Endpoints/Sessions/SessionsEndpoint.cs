using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    internal class SessionsEndpoint : PostGetAllEndpoint<GetSessionModel, PostSessionModel>, ISessionsEndpoint
    {
        public SessionsEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) : 
            base(httpClient, routeBuilder)
        {
            routeBuilder.AddEndpoint("Sessions");
        }

        ISessionByIdEndpoint IWithIdEndpoint<ISessionByIdEndpoint>.WithId(long id)
        {
            return new SessionByIdEndpoint(HttpClient, RouteBuilder, id);
        }
    }
}
