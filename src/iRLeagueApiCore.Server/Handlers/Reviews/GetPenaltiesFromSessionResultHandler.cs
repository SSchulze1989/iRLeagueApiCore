using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record GetPenaltiesFromSessionResultRequest(long SessionResultId) : IRequest<IEnumerable<PenaltyModel>>;
public sealed class GetPenaltiesFromSessionResultHandler : ReviewsHandlerBase<GetPenaltiesFromSessionResultHandler, GetPenaltiesFromSessionResultRequest, IEnumerable<PenaltyModel>>
{
    public GetPenaltiesFromSessionResultHandler(ILogger<GetPenaltiesFromSessionResultHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetPenaltiesFromSessionResultRequest>> validators, IResultCalculationQueue resultCalculationQueue) : base(logger, dbContext, validators, resultCalculationQueue)
    {
    }

    public override async Task<IEnumerable<PenaltyModel>> Handle(GetPenaltiesFromSessionResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getReviews = await GetReviewPenaltiesFromSessionResult(request.SessionResultId, cancellationToken);
        return getReviews;
    }

    private async Task<IEnumerable<PenaltyModel>> GetReviewPenaltiesFromSessionResult(long sessionResultId, CancellationToken cancellationToken)
    {
        var addPenalties = await dbContext.AddPenaltys
            .Where(x => x.ScoredResultRow.SessionResultId == sessionResultId)
            .Select(MapToAddPenaltyModelExpression)
            .ToListAsync(cancellationToken);

        var reviewPenalties = await dbContext.ReviewPenaltys
            .Where(x => x.ResultRow.SessionResultId == sessionResultId)
            .Select(MapToReviewPenaltyModelExpression)
            .ToListAsync(cancellationToken);

        return addPenalties.Concat(reviewPenalties);
    }
}
