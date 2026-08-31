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
        var resultTask = memoryCache.GetOrCreate(cacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheKeys.ResultsCacheDuration;
            return MapToGetResultModelsFromEventAsync(request.EventId, CancellationToken.None);
        })!;
        var getResults = await resultTask;
        if (!getResults.Any())
        {
            memoryCache.Remove(cacheKey);
            throw new ResourceNotFoundException();
        }
        return getResults;
    }
}
