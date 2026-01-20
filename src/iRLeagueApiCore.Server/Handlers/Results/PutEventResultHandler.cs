using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record PutEventResultRequest(LeagueUser User, long EventId, RawEventResultModel Model) : IRequest<RawEventResultModel>;
public class PutEventResultHandler : ResultHandlerBase<PutEventResultHandler, PutEventResultRequest, RawEventResultModel>
{
    public PutEventResultHandler(ILogger<PutEventResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutEventResultRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<RawEventResultModel> Handle(PutEventResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var eventResult = await GetOrCreateEventResultEntity(request.EventId, request.User, cancellationToken);
        eventResult = await MapToEventResultEntity(eventResult, request.Model, request.User, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getEventResult = await dbContext.EventResults
            .Select(MapToRawEventResultModelExpression)
            .FirstOrDefaultAsync(x => x.EventId == request.EventId, cancellationToken)
            ?? throw new InvalidOperationException("Updated resource was not found");
        return getEventResult;
    }

    private async Task<EventResultEntity> GetOrCreateEventResultEntity(long eventId, LeagueUser user, CancellationToken cancellationToken)
    {
        var eventResult = await dbContext.EventResults
            .Where(x => x.EventId == eventId)
            .Include(x => x.SessionResults)
                .ThenInclude(x => x.ResultRows)
            .FirstOrDefaultAsync(cancellationToken);
        if (eventResult is null)
        {
            eventResult = CreateVersionEntity<EventResultEntity>(user, new());
            eventResult.LeagueId = dbContext.LeagueProvider.LeagueId;
            var @event = await dbContext.Events
                .FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            @event.EventResult = eventResult;
        }
        return eventResult;
    }

    private async Task<EventResultEntity> MapToEventResultEntity(EventResultEntity entity, RawEventResultModel model, LeagueUser user, CancellationToken cancellationToken)
    {
        UpdateVersionEntity(user, entity);
        var sessionResults = new List<SessionResultEntity>();
        foreach (var sessionResultModel in model.SessionResults)
        {
            var sessionResult = entity.SessionResults.FirstOrDefault(x => x.SessionId == sessionResultModel.SessionId);
            if (sessionResult is null)
            {
                sessionResult = CreateVersionEntity<SessionResultEntity>(user, new());
                sessionResult.LeagueId = dbContext.LeagueProvider.LeagueId;
                var session = await dbContext.Sessions
                    .FirstOrDefaultAsync(x => x.SessionId == sessionResultModel.SessionId, cancellationToken)
                    ?? throw new InvalidOperationException("Invalid session id");
                session.SessionResult = sessionResult;
            }
            sessionResult = await MapToSessionResultEntity(sessionResult, sessionResultModel, user, cancellationToken);
            sessionResults.Add(sessionResult);
        }
        entity.SessionResults = sessionResults;
        entity.RequiresRecalculation = true;
        return entity;
    }

    private async Task<SessionResultEntity> MapToSessionResultEntity(SessionResultEntity entity, RawSessionResultModel model, LeagueUser user, CancellationToken cancellationToken)
    {
        UpdateVersionEntity(user, entity);
        foreach (var resultRowModel in model.ResultRows)
        {
            var resultRow = entity.ResultRows.FirstOrDefault(x => x.ResultRowId == resultRowModel.ResultRowId && resultRowModel.ResultRowId > 0);
            if (resultRow is null)
            {
                resultRow = new()
                {
                    LeagueId = dbContext.LeagueProvider.LeagueId
                };
                entity.ResultRows.Add(resultRow);
            }
            resultRow = await MapToResultRowEntity(resultRow, resultRowModel, cancellationToken);
        }
        return entity;
    }
}

