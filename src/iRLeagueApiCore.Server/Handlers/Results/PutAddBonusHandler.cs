using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record PutAddBonusRequest(long AddBonusId, PutAddBonusModel Bonus) : IRequest<AddBonusModel>;

public class PutAddBonusHandler : ResultHandlerBase<PutAddBonusHandler, PutAddBonusRequest>, IRequestHandler<PutAddBonusRequest, AddBonusModel>
{
    public PutAddBonusHandler(ILogger<PutAddBonusHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutAddBonusRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<AddBonusModel> Handle(PutAddBonusRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken).ConfigureAwait(false);

        var putBonus = await GetAddBonusEntity(request.AddBonusId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        putBonus = MapToAddBonusEntity(putBonus, request.Bonus);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var getBonus = await MapToAddBonusModel(putBonus.AddBonusId, cancellationToken)
            ?? throw new InvalidOperationException("Updated resource not found");
        return getBonus;
    }
}
