using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetResultsFromSeasonRequest(long SeasonId) : IRequest<IEnumerable<SeasonEventResultModel>>;

public sealed class GetResultsFromSeasonHandler : ResultHandlerBase<GetResultsFromSeasonHandler, GetResultsFromSeasonRequest, IEnumerable<SeasonEventResultModel>>
{
    private readonly IMemoryCache memoryCache;

    public GetResultsFromSeasonHandler(ILogger<GetResultsFromSeasonHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetResultsFromSeasonRequest>> validators, IMemoryCache memoryCache) :
        base(logger, dbContext, validators)
    {
        this.memoryCache = memoryCache;
    }

    public override async Task<IEnumerable<SeasonEventResultModel>> Handle(GetResultsFromSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var cacheKey = CacheKeys.GetResultsBySeasonKey(request.SeasonId);
        var resultTask = memoryCache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheKeys.ResultsCacheDuration;
            return MapToGetResultModelsFromSeasonAsync(request.SeasonId, CancellationToken.None);
        })!;
        var getResults = await resultTask;
        if (!getResults.Any())
        {
            memoryCache.Remove(cacheKey);
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
