using System.Net;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record FetchResultsFromIRacingAPIRequest(long EventId, string IRSubsessionId) : IRequest<bool>;

public class FetchResultsFromIRacingAPIHandler : HandlerBase<FetchResultsFromIRacingAPIHandler, FetchResultsFromIRacingAPIRequest>, 
    IRequestHandler<FetchResultsFromIRacingAPIRequest, bool>
{
    private readonly ICredentials credentials;

    public FetchResultsFromIRacingAPIHandler(ILogger<FetchResultsFromIRacingAPIHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<FetchResultsFromIRacingAPIRequest>> validators, ICredentials credentials)
        : base(logger, dbContext, validators)
    {
        this.credentials = credentials;
    }

    public async Task<bool> Handle(FetchResultsFromIRacingAPIRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var @event = GetResultEventEntityAsync(request.EventId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return true;
    }

    private async Task<EventEntity?> GetResultEventEntityAsync(long eventId, CancellationToken cancellationToken)
    {
        return await dbContext.Events
            .Include(x => x.EventResult)
                .ThenInclude(x => x.SessionResults)
                    .ThenInclude(x => x.IRSimSessionDetails)
            .Where(x => x.EventId == eventId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}
