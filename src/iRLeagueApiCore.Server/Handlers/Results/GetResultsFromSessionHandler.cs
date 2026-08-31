using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using Microsoft.Extensions.Caching.Memory;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetResultsFromEventRequest(long EventId) : IRequest<IEnumerable<EventResultModel>>;

public sealed class GetResultsFromSessionHandler : ResultHandlerBase<GetResultsFromSessionHandler, GetResultsFromEventRequest, IEnumerable<EventResultModel>>
{
    private readonly IMemoryCache memoryCache;

    public GetResultsFromSessionHandler(ILogger<GetResultsFromSessionHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetResultsFromEventRequest>> validators, IMemoryCache memoryCache) :
        base(logger, dbContext, validators)
    {
        this.memoryCache = memoryCache;
    }

    public override async Task<IEnumerable<EventResultModel>> Handle(GetResultsFromEventRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var cacheKey = CacheKeys.GetResultsByEventKey(request.EventId);
        if (memoryCache.TryGetValue(cacheKey, out IEnumerable<EventResultModel>? cached) && cached is not null)
        {
            return cached;
        }
        var getResults = await MapToGetResultModelsFromEventAsync(request.EventId, cancellationToken);
        if (getResults.Count() == 0)
        {
            throw new ResourceNotFoundException();
        }
        memoryCache.Set(cacheKey, getResults, CacheKeys.ResultsCacheDuration);
        return getResults;
    }
}
