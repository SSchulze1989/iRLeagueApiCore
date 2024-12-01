using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record ModifyResultRowRequest(long ResultRowId, RawResultRowModel Row, bool TriggerCalculation = false) : IRequest<RawResultRowModel>;

public class ModifyResultRowHandler : ResultHandlerBase<ModifyResultRowHandler, ModifyResultRowRequest, RawResultRowModel>
{
    private readonly IResultCalculationQueue calculationQueue;

    public ModifyResultRowHandler(ILogger<ModifyResultRowHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<ModifyResultRowRequest>> validators,
        IResultCalculationQueue calculationQueue) : base(logger, dbContext, validators)
    {
        this.calculationQueue = calculationQueue;
    }

    public override async Task<RawResultRowModel> Handle(ModifyResultRowRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var row = await GetResultRowEntityAsync(request.ResultRowId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        await MapToResultRowEntity(row, request.Row, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        if (request.TriggerCalculation)
        {
            var eventId = await dbContext.ResultRows
                .Where(x => x.ResultRowId == row.ResultRowId)
                .Select(x => x.SubResult.EventId)
                .FirstOrDefaultAsync(cancellationToken);
            calculationQueue.QueueEventResultDebounced(eventId, 1000);
        }

        return MapToModResultRowModel(request.Row, row);
    }

    private async Task<ResultRowEntity?> GetResultRowEntityAsync(long resultRowId, CancellationToken cancellationToken)
    {
        return await dbContext.ResultRows.
            FirstOrDefaultAsync(x => resultRowId == x.ResultRowId, cancellationToken);
    }
}
