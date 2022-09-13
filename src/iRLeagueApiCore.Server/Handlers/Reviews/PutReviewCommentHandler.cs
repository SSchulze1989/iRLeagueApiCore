using FluentValidation;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record PutReviewCommentRequest(long LeagueId, long CommentId, LeagueUser User, PutReviewCommentModel Model) : IRequest<ReviewCommentModel>;

    public class PutReviewCommentHandler : CommentHandlerBase<PutReviewCommentHandler, PutReviewCommentRequest>, 
        IRequestHandler<PutReviewCommentRequest, ReviewCommentModel>
    {
        public PutReviewCommentHandler(ILogger<PutReviewCommentHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutReviewCommentRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ReviewCommentModel> Handle(PutReviewCommentRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putComment = await GetCommentEntityAsync(request.LeagueId, request.CommentId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            putComment = await MapToReviewCommentEntityAsync(request.User, request.Model, putComment, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getComment = await MapToReviewCommentModelAsync(putComment.LeagueId, putComment.CommentId, cancellationToken);
            return getComment;
        }
    }
}
