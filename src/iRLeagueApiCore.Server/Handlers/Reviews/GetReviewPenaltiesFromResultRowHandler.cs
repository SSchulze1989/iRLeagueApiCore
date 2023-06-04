using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record GetReviewPenaltiesFromResultRowRequest(long ScoredResultRowId) : IRequest<IEnumerable<ReviewPenaltyModel>>;
public sealed class GetReviewPenaltiesFromResultRowHandler : ReviewsHandlerBase<GetReviewPenaltiesFromResultRowHandler, GetReviewPenaltiesFromResultRowRequest>,
    IRequestHandler<GetReviewPenaltiesFromResultRowRequest, IEnumerable<ReviewPenaltyModel>>
{
    public GetReviewPenaltiesFromResultRowHandler(ILogger<GetReviewPenaltiesFromResultRowHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetReviewPenaltiesFromResultRowRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<ReviewPenaltyModel>> Handle(GetReviewPenaltiesFromResultRowRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getReviews = await GetReviewPenaltiesFromResultRow(request.ScoredResultRowId, cancellationToken);
        return getReviews;
    }

    private async Task<IEnumerable<ReviewPenaltyModel>> GetReviewPenaltiesFromResultRow(long ScoredResultRowId, CancellationToken cancellationToken)
    {
        return await dbContext.ReviewPenaltys
            .Where(x => x.ResultRowId == ScoredResultRowId)
            .Select(MapToReviewPenaltyModelExpression)
            .ToListAsync(cancellationToken);
    }
}
