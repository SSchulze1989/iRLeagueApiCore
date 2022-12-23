﻿using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record GetReviewsFromSessionRequest(long LeagueId, long SessionId, bool IncludeComments) : IRequest<IEnumerable<ReviewModel>>;
public class GetReviewsFromSessionHandler : ReviewsHandlerBase<GetReviewsFromSessionHandler, GetReviewsFromSessionRequest>,
    IRequestHandler<GetReviewsFromSessionRequest, IEnumerable<ReviewModel>>
{
    public GetReviewsFromSessionHandler(ILogger<GetReviewsFromSessionHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetReviewsFromSessionRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<ReviewModel>> Handle(GetReviewsFromSessionRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getReviews = await MapToGetReviewsFromSessionAsync(request.LeagueId, request.SessionId, request.IncludeComments, cancellationToken);
        return getReviews;
    }

    private async Task<IEnumerable<ReviewModel>> MapToGetReviewsFromSessionAsync(long leagueId, long sessionId, bool includeComments, CancellationToken cancellationToken)
    {
        return await dbContext.IncidentReviews
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SessionId == sessionId)
            .Select(MapToReviewModelExpression(includeComments))
            .ToListAsync(cancellationToken);
    }
}
