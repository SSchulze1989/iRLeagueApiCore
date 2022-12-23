namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record DeleteReviewCommentRequest(long LeagueId, long CommentId) : IRequest;

    public class DeleteReviewCommentHandler : CommentHandlerBase<DeleteReviewCommentHandler, DeleteReviewCommentRequest>,
        IRequestHandler<DeleteReviewCommentRequest>
    {
        public DeleteReviewCommentHandler(ILogger<DeleteReviewCommentHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteReviewCommentRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteReviewCommentRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var deleteComment = await GetCommentEntityAsync(request.LeagueId, request.CommentId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            dbContext.Remove(deleteComment);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
