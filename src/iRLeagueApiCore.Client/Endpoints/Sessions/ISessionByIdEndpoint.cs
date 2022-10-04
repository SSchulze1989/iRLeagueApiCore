using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface ISessionByIdEndpoint
    {
        public IPostEndpoint<ReviewModel, PostReviewModel> Reviews();
    }
}