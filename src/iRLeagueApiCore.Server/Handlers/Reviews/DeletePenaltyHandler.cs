using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record DeletePenaltyRequest(long PenaltyId) : IRequest<Unit>;

public class DeletePenaltyHandler : ReviewsHandlerBase<DeletePenaltyHandler,  DeletePenaltyRequest, Unit>
{
    public DeletePenaltyHandler(ILogger<DeletePenaltyHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeletePenaltyRequest>> validators, 
        IResultCalculationQueue resultCalculationQueue) : base(logger, dbContext, validators, resultCalculationQueue)
    {
    }

    public override async Task<Unit> Handle(DeletePenaltyRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deletePenalty = await GetDeleteAddPenaltyEntity(request.PenaltyId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.Remove(deletePenalty);
        await dbContext.SaveChangesAsync(cancellationToken);

        resultCalculationQueue.QueueEventResultDebounced(deletePenalty.ScoredResultRow.ScoredSessionResult.ScoredEventResult.EventId, penaltyCalcDebounceMs);
        return Unit.Value;
    }

    private async Task<AddPenaltyEntity?> GetDeleteAddPenaltyEntity(long penaltyId, CancellationToken cancellationToken)
    {
        return await dbContext.AddPenaltys
            .Include(x => x.ScoredResultRow)
                .ThenInclude(x => x.ScoredSessionResult)
                    .ThenInclude(x => x.ScoredEventResult)
            .FirstOrDefaultAsync(x => x.AddPenaltyId == penaltyId, cancellationToken);
    }
}
