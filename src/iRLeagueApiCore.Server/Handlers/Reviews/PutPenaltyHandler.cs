using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record PutPenaltyRequest(long PenaltyId, PutPenaltyModel Model) : IRequest<PenaltyModel>;

public class PutPenaltyHandler : ReviewsHandlerBase<PutPenaltyHandler, PutPenaltyRequest, PenaltyModel>
{
    public PutPenaltyHandler(ILogger<PutPenaltyHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutPenaltyRequest>> validators,
        IResultCalculationQueue resultCalculationQueue) : base(logger, dbContext, validators, resultCalculationQueue)
    {
    }

    public override async Task<PenaltyModel> Handle(PutPenaltyRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var putPenalty = await GetAddPenaltyEntity(request.PenaltyId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        putPenalty = await MapToAddPenaltyEntity(request.Model, putPenalty, cancellationToken);
        var changed = await dbContext.SaveChangesAsync(cancellationToken);
        var getPenalty = await MapToAddPenaltyModel(putPenalty.AddPenaltyId, cancellationToken)
            ?? throw new InvalidOperationException("Updated resource not found");
        if (changed > 0)
        {
            resultCalculationQueue.QueueEventResultDebounced(getPenalty.EventId, penaltyCalcDebounceMs);
        }
        return getPenalty;
    }
}
