using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record PostReviewCommentToReviewRequest(long LeagueId, long ReviewId, LeagueUser User, PostReviewCommentModel Model) : IRequest<ReviewCommentModel>;

    public class PostReviewCommentToReviewHandler : CommentHandlerBase<PostReviewCommentToReviewHandler, PostReviewCommentToReviewRequest>,
        IRequestHandler<PostReviewCommentToReviewRequest, ReviewCommentModel>
    {
        public PostReviewCommentToReviewHandler(ILogger<PostReviewCommentToReviewHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostReviewCommentToReviewRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<ReviewCommentModel> Handle(PostReviewCommentToReviewRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var postComment = await CreateCommentEntityOnReviewAsync(request.LeagueId, request.ReviewId, request.User, cancellationToken);
            postComment = await MapToReviewCommentEntityAsync(request.User, request.Model, postComment, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getComment = await MapToReviewCommentModelAsync(postComment.LeagueId, postComment.CommentId, cancellationToken)
                ?? throw new InvalidOperationException("Created resource was not found");
            return getComment;
        }

        protected virtual async Task<ReviewCommentEntity> CreateCommentEntityOnReviewAsync(long leagueId, long reviewId, LeagueUser user,
            CancellationToken cancellationToken)
        {
            var review = await dbContext.IncidentReviews
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ReviewId == reviewId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ResourceNotFoundException();
            var commentEntity = CreateVersionEntity(user, new ReviewCommentEntity());
            commentEntity.AuthorName = user.Name;
            commentEntity.AuthorUserId = user.Id;
            review.Comments.Add(commentEntity);
            return commentEntity;
        }
    }
}
