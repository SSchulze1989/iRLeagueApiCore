using iRLeagueApiCore.Client.Endpoints.Reviews;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public class SessionByIdEndpoint : EndpointBase, ISessionByIdEndpoint
    {
        public SessionByIdEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder, long sessionId) : 
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddParameter(sessionId);
        }

        public IPostEndpoint<ReviewModel, PostReviewModel> Reviews()
        {
            return new ReviewsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}