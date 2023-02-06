using iRLeagueApiCore.Common.Models.Championships;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record PostChampionshipRequest(long LeagueId, LeagueUser User, PostChampionshipModel Model) : IRequest<ChampionshipModel>;

public sealed class PostChampionshipHandler : ChampionshipHandlerBase<PostChampionshipHandler, PostChampionshipRequest>, 
    IRequestHandler<PostChampionshipRequest, ChampionshipModel>
{
    public PostChampionshipHandler(ILogger<PostChampionshipHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostChampionshipRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public async Task<ChampionshipModel> Handle(PostChampionshipRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postChampionship = await CreateChampionshipEntityAsync(request.LeagueId, request.User, cancellationToken);
        postChampionship = await MapToChampionshipEntityAsync(request.User, request.Model, postChampionship, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getChampionship = await MapToChampionshipModelAsync(request.LeagueId, postChampionship.ChampionshipId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getChampionship;
    }

    private async Task<ChampionshipEntity> CreateChampionshipEntityAsync(long leagueId, LeagueUser user, CancellationToken cancellationToken)
    {
        var league = await dbContext.Leagues
            .FirstOrDefaultAsync(x => x.Id == leagueId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        var championship = CreateVersionEntity(user, new ChampionshipEntity());
        league.Championships.Add(championship);
        dbContext.Championships.Add(championship);
        return championship;
    }
}
