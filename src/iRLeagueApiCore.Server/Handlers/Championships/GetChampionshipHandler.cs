using iRLeagueApiCore.Common.Models.Championships;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record GetChampionshipRequest(long LeagueId, long ChampionshipId) : IRequest<ChampionshipModel>;

public class GetChampionshipHandler : ChampionshipHandlerBase<GetChampionshipHandler, GetChampionshipRequest>, 
    IRequestHandler<GetChampionshipRequest, ChampionshipModel>
{
    public GetChampionshipHandler(ILogger<GetChampionshipHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetChampionshipRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public async Task<ChampionshipModel> Handle(GetChampionshipRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getChampionship = await MapToChampionshipModelAsync(request.LeagueId, request.ChampionshipId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getChampionship;
    }
}
