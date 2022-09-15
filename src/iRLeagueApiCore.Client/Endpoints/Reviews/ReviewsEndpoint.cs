using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.Reviews
{
    public class ReviewsEndpoint : GetAllEndpoint<ReviewModel>, IReviewsEndpoint, IGetAllEndpoint<ReviewModel>
    {
        public ReviewsEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) : 
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Reviews");
        }

        public IReviewByIdEndpoint WithId(long id)
        {
            return new ReviewByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
