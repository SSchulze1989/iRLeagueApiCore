

namespace iRLeagueApiCore.Server.Handlers.Results;

public record DeleteRawEventResultRequest(long EventId, bool DeleteAllScoredResults = false) : IRequest<Unit>;

public class DeleteRawEventResultHandler : ResultHandlerBase<DeleteRawEventResultHandler, DeleteRawEventResultRequest, Unit>
{
    public DeleteRawEventResultHandler(ILogger<DeleteRawEventResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteRawEventResultRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteRawEventResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var result = await dbContext.EventResults
            .FirstOrDefaultAsync(x => x.EventId == request.EventId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.Remove(result);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
