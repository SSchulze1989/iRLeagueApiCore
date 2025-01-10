using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record PostReviewToSessionRequest(long SessionId, LeagueUser User, PostReviewModel Model) : IRequest<ReviewModel>;
public sealed class PostReviewToSessionHandler : ReviewsHandlerBase<PostReviewToSessionHandler, PostReviewToSessionRequest, ReviewModel>
{
    /// <summary>
    /// Always include comments because this can only be called by an authorized user
    /// </summary>
    private const bool includeComments = true;

    public PostReviewToSessionHandler(ILogger<PostReviewToSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostReviewToSessionRequest>> validators,
        IResultCalculationQueue resultCalculationQueue) : base(logger, dbContext, validators, resultCalculationQueue)
    {
    }

    public override async Task<ReviewModel> Handle(PostReviewToSessionRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postReview = await CreateReviewEntityOnSession(request.SessionId, request.User, cancellationToken);
        postReview = await MapToReviewEntity(request.User, request.Model, postReview, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getReview = await MapToReviewModel(postReview.ReviewId, includeComments, cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        if (getReview.VoteResults.Any())
        {
            resultCalculationQueue.QueueEventResultDebounced(getReview.EventId, reviewCalcDebounceMs);
        }
        return getReview;
    }

    private async Task<IncidentReviewEntity> CreateReviewEntityOnSession(long sessionId, LeagueUser user, CancellationToken cancellationToken)
    {
        var session = await dbContext.Sessions
            .Where(x => x.SessionId == sessionId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var review = CreateVersionEntity(user, new IncidentReviewEntity());
        session.IncidentReviews.Add(review);
        return review;
    }
}
