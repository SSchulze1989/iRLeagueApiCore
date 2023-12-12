using iRLeagueApiCore.Common.Models.Results;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetAddBonusRequest(long AddBonusId) : IRequest<AddBonusModel>;

public class GetAddBonusHandler : ResultHandlerBase<GetAddBonusHandler, GetAddBonusRequest>, IRequestHandler<GetAddBonusRequest, AddBonusModel>
{
    public GetAddBonusHandler(ILogger<GetAddBonusHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetAddBonusRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<AddBonusModel> Handle(GetAddBonusRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken).ConfigureAwait(false);

        var getBonus = await MapToAddBonusModel(request.AddBonusId, cancellationToken).ConfigureAwait(false)
            ?? throw new ResourceNotFoundException();
        return getBonus;
    }
}
