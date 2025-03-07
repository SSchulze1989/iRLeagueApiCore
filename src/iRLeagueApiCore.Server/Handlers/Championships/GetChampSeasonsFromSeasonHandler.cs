﻿using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record GetChampSeasonsFromSeasonRequest(long SeasonId) : IRequest<IEnumerable<ChampSeasonModel>>;

public sealed class GetChampSeasonFromSeasonHandler : ChampSeasonHandlerBase<GetChampSeasonFromSeasonHandler, GetChampSeasonsFromSeasonRequest, IEnumerable<ChampSeasonModel>>
{
    public GetChampSeasonFromSeasonHandler(ILogger<GetChampSeasonFromSeasonHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetChampSeasonsFromSeasonRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<ChampSeasonModel>> Handle(GetChampSeasonsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getChampSeasons = await MapToChampSeasonModelsFromSeasonAsync(request.SeasonId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getChampSeasons;
    }

    private async Task<IEnumerable<ChampSeasonModel>> MapToChampSeasonModelsFromSeasonAsync(long seasonId, CancellationToken cancellationToken)
    {
        return await ChampSeasonsQuery()
            .Where(x => x.SeasonId == seasonId)
            .OrderBy(x => x.Index)
            .Select(MapToChampSeasonModelExpression)
            .ToListAsync(cancellationToken);
    }
}