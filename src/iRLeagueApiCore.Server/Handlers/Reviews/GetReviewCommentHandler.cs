using FluentValidation;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record GetReviewCommentRequest(long LeagueId, long CommentId) : IRequest<ReviewCommentModel>;

    public class GetReviewCommentHandler : CommentHandlerBase<GetReviewCommentHandler, GetReviewCommentRequest>,
        IRequestHandler<GetReviewCommentRequest, ReviewCommentModel>
    {
        public GetReviewCommentHandler(ILogger<GetReviewCommentHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetReviewCommentRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ReviewCommentModel> Handle(GetReviewCommentRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getComment = await MapToReviewCommentModelAsync(request.LeagueId, request.CommentId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getComment;
        }
    }
}
