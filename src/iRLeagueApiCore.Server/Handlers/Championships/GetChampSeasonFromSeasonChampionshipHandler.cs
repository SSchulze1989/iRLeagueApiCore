﻿using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Championships;

public record GetChampSeasonFromSeasonChampionshipRequest(long LeagueId, long SeasonId, long ChampionshipId) : IRequest<ChampSeasonModel>;

public class GetChampSeasonFromSeasonChampionshipHandler : ChampSeasonHandlerBase<GetChampSeasonFromSeasonChampionshipHandler, GetChampSeasonFromSeasonChampionshipRequest>,
    IRequestHandler<GetChampSeasonFromSeasonChampionshipRequest, ChampSeasonModel>
{
    public GetChampSeasonFromSeasonChampionshipHandler(ILogger<GetChampSeasonFromSeasonChampionshipHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetChampSeasonFromSeasonChampionshipRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public async Task<ChampSeasonModel> Handle(GetChampSeasonFromSeasonChampionshipRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getChampSeason = await GetChampSeasonFromSeasonChampionship(request.LeagueId, request.SeasonId, request.ChampionshipId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getChampSeason;
    }

    private async Task<ChampSeasonModel?> GetChampSeasonFromSeasonChampionship(long leagueId, long seasonId, long championshipId, CancellationToken cancellationToken)
    {
        return await dbContext.ChampSeasons
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SeasonId == seasonId)
            .Where(x => x.ChampionshipId == championshipId)
            .Select(MapToChampSeasonModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }
}