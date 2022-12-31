using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record GetProtestsFromEventRequest(long LeagueId, long EventId) : IRequest<IEnumerable<ProtestModel>>;

public class GetProtestsFromEventHandler : ProtestsHandlerBase<GetProtestsFromEventHandler, GetProtestsFromEventRequest>, IRequestHandler<GetProtestsFromEventRequest, 
    IEnumerable<ProtestModel>>
{
    public GetProtestsFromEventHandler(ILogger<GetProtestsFromEventHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetProtestsFromEventRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<ProtestModel>> Handle(GetProtestsFromEventRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getProtests = await MapToProtestModelsFromEvent(request.LeagueId, request.EventId, cancellationToken);
        return getProtests;
    }

    private async Task<IEnumerable<ProtestModel>> MapToProtestModelsFromEvent(long leagueId, long eventId, CancellationToken cancellationToken)
    {
        return await dbContext.Protests
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.Session.EventId == eventId)
            .Select(MapToProtestModelExpression)
            .ToListAsync(cancellationToken);
    }
}
