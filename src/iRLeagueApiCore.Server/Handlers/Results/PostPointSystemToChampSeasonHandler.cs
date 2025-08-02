using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record PostPointSystemToChampSeasonRequest(long ChampSeasonId, LeagueUser User, PostPointSystemModel Model) : IRequest<PointSystemModel>;

public sealed class PostPointSystemToChampSeasonHandler : PointSystemHandlerBase<PostPointSystemToChampSeasonHandler, PostPointSystemToChampSeasonRequest, PointSystemModel>
{
    public PostPointSystemToChampSeasonHandler(ILogger<PostPointSystemToChampSeasonHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<PostPointSystemToChampSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<PointSystemModel> Handle(PostPointSystemToChampSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postResultConfig = await CreateResultConfigEntity(request.ChampSeasonId, request.User, cancellationToken);
        postResultConfig = await MapToResultConfigEntityAsync(request.User, request.Model, postResultConfig, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getResultConfig = await MapToResultConfigModel(postResultConfig.PointSystemId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getResultConfig;
    }

    private async Task<PointSystemEntity> CreateResultConfigEntity(long champSeasonId, LeagueUser user, CancellationToken cancellationToken)
    {
        var champSeason = await dbContext.ChampSeasons
            .Where(x => x.ChampSeasonId == champSeasonId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var resultConfig = CreateVersionEntity(user, new PointSystemEntity());
        champSeason.PointSystems.Add(resultConfig);
        return await Task.FromResult(resultConfig);
    }
}
