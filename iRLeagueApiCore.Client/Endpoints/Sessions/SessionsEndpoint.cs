using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    internal class SessionsEndpoint : PostGetAllEndpoint<SessionModel, PostSessionModel>, ISessionsEndpoint
    {
        public SessionsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Sessions");
        }

        ISessionByIdEndpoint IWithIdEndpoint<ISessionByIdEndpoint>.WithId(long id)
        {
            return new SessionByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
