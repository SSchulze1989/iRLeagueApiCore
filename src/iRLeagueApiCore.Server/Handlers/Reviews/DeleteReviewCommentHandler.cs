namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record DeleteReviewCommentRequest(long CommentId) : IRequest<Unit>;

public sealed class DeleteReviewCommentHandler : CommentHandlerBase<DeleteReviewCommentHandler, DeleteReviewCommentRequest, Unit>
{
    public DeleteReviewCommentHandler(ILogger<DeleteReviewCommentHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteReviewCommentRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteReviewCommentRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteComment = await GetCommentEntityAsync(request.CommentId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.Remove(deleteComment);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
