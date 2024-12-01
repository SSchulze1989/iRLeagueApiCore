using iRLeagueApiCore.Server.Handlers.Reviews;

namespace iRLeagueApiCore.Server.Validation.Reviews;

public class MoveReviewToSessionRequestValidator : AbstractValidator<MoveReviewToSessionRequest>
{
    private readonly LeagueDbContext dbContext;

    public MoveReviewToSessionRequestValidator(LeagueDbContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(x => x.SessionId)
            .MustAsync(SessionIdBelongToSameEvent)
            .WithMessage("Session id must belong to the same event as the previous session this review was attached to.");
    }

    private async Task<bool> SessionIdBelongToSameEvent(MoveReviewToSessionRequest request, long sessionId, CancellationToken cancellationToken)
    {
        var reviewEventId = (await dbContext.IncidentReviews
            .Where(x => x.ReviewId == request.ReviewId)
            .Select(x => x.Session.EventId)
            .FirstOrDefaultAsync(cancellationToken));
        return await dbContext.Sessions
            .AnyAsync(x => x.SessionId == sessionId && x.EventId == reviewEventId, cancellationToken);
    }
}
