namespace iRLeagueApiCore.Server.Handlers.Results;

public record DeletePointSystemRequest(long ResultConfigId) : IRequest<Unit>;

public sealed class DeletePointSystemHandler : PointSystemHandlerBase<DeletePointSystemHandler, DeletePointSystemRequest, Unit>
{
    public DeletePointSystemHandler(ILogger<DeletePointSystemHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<DeletePointSystemRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeletePointSystemRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteResultConfig = await GetResultConfigEntity(request.ResultConfigId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.ResultConfigurations.Remove(deleteResultConfig);
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
