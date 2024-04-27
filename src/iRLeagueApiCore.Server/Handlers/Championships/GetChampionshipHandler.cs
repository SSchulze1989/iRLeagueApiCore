using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record GetChampionshipRequest(long ChampionshipId) : IRequest<ChampionshipModel>;

public class GetChampionshipHandler : ChampionshipHandlerBase<GetChampionshipHandler,  GetChampionshipRequest, ChampionshipModel>
{
    public GetChampionshipHandler(ILogger<GetChampionshipHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetChampionshipRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public override async Task<ChampionshipModel> Handle(GetChampionshipRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getChampionship = await MapToChampionshipModelAsync(request.ChampionshipId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getChampionship;
    }
}
