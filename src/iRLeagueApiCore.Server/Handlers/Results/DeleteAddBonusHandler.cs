

namespace iRLeagueApiCore.Server.Handlers.Results;

public record DeleteAddBonusRequest(long AddBonusId) : IRequest;

public class DeleteAddBonusHandler : ResultHandlerBase<DeleteAddBonusHandler, DeleteAddBonusRequest>, IRequestHandler<DeleteAddBonusRequest, Unit>
{
    public DeleteAddBonusHandler(ILogger<DeleteAddBonusHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteAddBonusRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<Unit> Handle(DeleteAddBonusRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken).ConfigureAwait(false);

        var deleteAddBonus = await GetAddBonusEntity(request.AddBonusId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        dbContext.AddBonuses.Remove(deleteAddBonus);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return Unit.Value;
    }
}
