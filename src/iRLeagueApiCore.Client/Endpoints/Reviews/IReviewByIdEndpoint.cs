using iRLeagueApiCore.Common.Models.Reviews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Reviews
{
    public interface IReviewByIdEndpoint : IUpdateEndpoint<ReviewModel, PutReviewModel>
    {
        public IPostEndpoint<ReviewCommentModel, PostReviewCommentModel> ReviewComments();
    }
}
