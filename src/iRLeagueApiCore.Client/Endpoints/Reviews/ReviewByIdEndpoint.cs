using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models.Reviews;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Reviews
{
    public class ReviewByIdEndpoint : UpdateEndpoint<ReviewModel, PutReviewModel>, IReviewByIdEndpoint
    {
        public ReviewByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long id) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddParameter(id);
        }

        public IPostEndpoint<ReviewCommentModel, PostReviewCommentModel> ReviewComments()
        {
            return new ReviewCommentsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}
