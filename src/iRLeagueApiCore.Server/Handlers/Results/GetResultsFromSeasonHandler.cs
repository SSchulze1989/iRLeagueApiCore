﻿using iRLeagueApiCore.Common.Models;
namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetResultsFromSeasonRequest(long SeasonId) : IRequest<IEnumerable<SeasonEventResultModel>>;

public sealed class GetResultsFromSeasonHandler : ResultHandlerBase<GetResultsFromSeasonHandler, GetResultsFromSeasonRequest, IEnumerable<SeasonEventResultModel>>
{
    public GetResultsFromSeasonHandler(ILogger<GetResultsFromSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultsFromSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<SeasonEventResultModel>> Handle(GetResultsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getResults = await MapToGetResultModelsFromSeasonAsync(request.SeasonId, cancellationToken);
        if (getResults.Count() == 0)
        {
            throw new ResourceNotFoundException();
        }
        return getResults;
    }

    private async Task<IEnumerable<SeasonEventResultModel>> MapToGetResultModelsFromSeasonAsync(long seasonId, CancellationToken cancellationToken)
    {
        var seasonResults = (await dbContext.ScoredEventResults
            .Where(x => x.Event.Schedule.SeasonId == seasonId)
            .OrderBy(x => x.ResultConfigId)
            .Select(MapToEventResultModelExpression)
            .OrderBy(x => x.Index)
            .ToListAsync(cancellationToken))
            .OrderBy(x => x.Date)
            .ToList();

        var groupedResults = seasonResults.GroupBy(x => x.EventId);
        var seasonEventResults = groupedResults.Select(x => new SeasonEventResultModel()
        {
            EventId = x.Key,
            EventName = x.FirstOrDefault()?.EventName ?? string.Empty,
            ConfigName = x.FirstOrDefault()?.ConfigName ?? string.Empty,
            TrackName = x.FirstOrDefault()?.TrackName ?? string.Empty,
            EventResults = x,
        }).ToList();
        return seasonEventResults;
    }
}
