using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record PostResultConfigRequest(long LeagueId, LeagueUser User, PostResultConfigModel Model) : IRequest<ResultConfigModel>;

public sealed class PostResultConfigHandler : ResultConfigHandlerBase<PostResultConfigHandler, PostResultConfigRequest>,
    IRequestHandler<PostResultConfigRequest, ResultConfigModel>
{
    public PostResultConfigHandler(ILogger<PostResultConfigHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostResultConfigRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<ResultConfigModel> Handle(PostResultConfigRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postResultConfig = await CreateResultConfigEntity(request.LeagueId, request.User, cancellationToken);
        postResultConfig = await MapToResultConfigEntityAsync(request.LeagueId, request.User, request.Model, postResultConfig, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getResultConfig = await MapToResultConfigModel(postResultConfig.LeagueId, postResultConfig.ResultConfigId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getResultConfig;
    }

    private async Task<ResultConfigurationEntity> CreateResultConfigEntity(long leagueId, LeagueUser user, CancellationToken cancellationToken)
    {
        var league = await GetLeagueEntityAsync(leagueId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        var resultConfig = CreateVersionEntity(user, new ResultConfigurationEntity());
        league.ResultConfigs.Add(resultConfig);
        return await Task.FromResult(resultConfig);
    }
}
