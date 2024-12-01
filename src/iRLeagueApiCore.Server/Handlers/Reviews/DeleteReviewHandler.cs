using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record DeleteReviewRequest(long ReviewId) : IRequest;

public sealed class DeleteReviewHandler : ReviewsHandlerBase<DeleteReviewHandler, DeleteReviewRequest, Unit>
{
    public DeleteReviewHandler(ILogger<DeleteReviewHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteReviewRequest>> validators,
        IResultCalculationQueue resultCalculationQueue) : base(logger, dbContext, validators, resultCalculationQueue)
    {
    }

    public override async Task<Unit> Handle(DeleteReviewRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteReview = await GetReviewEntity(request.ReviewId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        var eventId = deleteReview.Session.EventId;
        dbContext.Remove(deleteReview);
        await dbContext.SaveChangesAsync(cancellationToken);
        if (deleteReview.AcceptedReviewVotes.Any())
        {
            resultCalculationQueue.QueueEventResultDebounced(eventId, reviewCalcDebounceMs);
        }
        return Unit.Value;
    }
}
