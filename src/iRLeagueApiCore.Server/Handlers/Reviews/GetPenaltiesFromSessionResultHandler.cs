using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record GetPenaltiesFromSessionResultRequest(long SessionResultId) : IRequest<IEnumerable<PenaltyModel>>;
public sealed class GetPenaltiesFromSessionResultHandler : ReviewsHandlerBase<GetPenaltiesFromSessionResultHandler, GetPenaltiesFromSessionResultRequest>,
    IRequestHandler<GetPenaltiesFromSessionResultRequest, IEnumerable<PenaltyModel>>
{
    public GetPenaltiesFromSessionResultHandler(ILogger<GetPenaltiesFromSessionResultHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetPenaltiesFromSessionResultRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<PenaltyModel>> Handle(GetPenaltiesFromSessionResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getReviews = await GetReviewPenaltiesFromSessionResult(request.SessionResultId, cancellationToken);
        return getReviews;
    }

    private async Task<IEnumerable<PenaltyModel>> GetReviewPenaltiesFromSessionResult(long sessionResultId, CancellationToken cancellationToken)
    {
        // It is required to fetch the whole entity here first because if used witht he expression directly the 
        // value of "Value.Time" is not converted to a TimeSpan and will always have the default value
        // It is not ideal to fetch the whole entities - including ScoredResultRow - but I could not find another workaround
        // --> this might be solved by updating to EF Core 7 with Pomelo
        var addPenalties = (await dbContext.AddPenaltys
            .Include(x => x.ScoredResultRow.Member)
            .Where(x => x.ScoredResultRow.SessionResultId == sessionResultId)
            .ToListAsync(cancellationToken))
            .Select(MapToAddPenaltyModelExpression.Compile())
            .ToList();

        var reviewPenalties = (await dbContext.ReviewPenaltys
            .Include(x => x.ResultRow.Member)
            .Where(x => x.ResultRow.SessionResultId == sessionResultId)
            .ToListAsync(cancellationToken))
            .Select(MapToReviewPenaltyModelExpression.Compile())
            .ToList();

        return addPenalties.Concat(reviewPenalties);
    }
}
