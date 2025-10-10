using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.Services.TriggerService.Events;
public record ResultCalculatedEventNotification(long EventId) : INotification;

public sealed class ResultCalculatedEventHandler : EventHandlerBase<ResultCalculatedEventNotification>
{
    public ResultCalculatedEventHandler(TriggerHostedService triggerHostedService, LeagueDbContext dbContext) : 
        base(triggerHostedService, dbContext)
    {
    }

    protected override TriggerEventType TriggerEventType => TriggerEventType.ResultCalculated;

    protected override Task<TriggerParameterModel> GetParameters(ResultCalculatedEventNotification notification, CancellationToken cancellationToken)
    {
        var parameters = new TriggerParameterModel
        {
            EventType = TriggerEventType.ResultCalculated,
            RefId1 = notification.EventId,
        };
        return Task.FromResult(parameters);
    }
}


