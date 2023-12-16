using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetBonusesFromSessionResultRequest(long SessionResultId) : IRequest<IEnumerable<AddBonusModel>>;

public class GetBonusesFromSessionResultHandler : ResultHandlerBase<GetBonusesFromSessionResultHandler, GetBonusesFromSessionResultRequest>,
    IRequestHandler<GetBonusesFromSessionResultRequest, IEnumerable<AddBonusModel>>
{
    public GetBonusesFromSessionResultHandler(ILogger<GetBonusesFromSessionResultHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetBonusesFromSessionResultRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<AddBonusModel>> Handle(GetBonusesFromSessionResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken).ConfigureAwait(false);
        var getBonuses = await GetBonusesFromSessionResult(request.SessionResultId, cancellationToken).ConfigureAwait(false);
        return getBonuses;
    }

    private async Task<IEnumerable<AddBonusModel>> GetBonusesFromSessionResult(long sessionResultId, CancellationToken cancellationToken)
    {
        return await dbContext.AddBonuses
            .Where(x => x.ScoredResultRow.SessionResultId == sessionResultId)
            .Select(MapToAddBonusModelExpression)
            .ToListAsync(cancellationToken);
    }
}
