﻿using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record GetReviewsFromEventRequest(long LeagueId, long EventId, bool IncludeComments) : IRequest<IEnumerable<ReviewModel>>;
public class GetReviewsFromEventHandler : ReviewsHandlerBase<GetReviewsFromEventHandler, GetReviewsFromEventRequest>,
    IRequestHandler<GetReviewsFromEventRequest, IEnumerable<ReviewModel>>
{
    public GetReviewsFromEventHandler(ILogger<GetReviewsFromEventHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetReviewsFromEventRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<ReviewModel>> Handle(GetReviewsFromEventRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getReviews = await MapToGetReviewsFromEventAsync(request.LeagueId, request.EventId, request.IncludeComments, cancellationToken);
        return getReviews.OrderBy(x => x.SessionNr).ThenBy(x => x.IncidentNr);
    }

    private async Task<IEnumerable<ReviewModel>> MapToGetReviewsFromEventAsync(long leagueId, long EventId, bool includeComments, CancellationToken cancellationToken)
    {
        return await dbContext.IncidentReviews
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.Session.EventId == EventId)
            .Select(MapToReviewModelExpression(includeComments))
            .ToListAsync(cancellationToken);
    }
}
