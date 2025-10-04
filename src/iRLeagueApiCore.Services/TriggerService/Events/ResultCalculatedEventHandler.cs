using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.Services.TriggerService.Events;
public record ResultCalculatedEventNotification(long EventId) : INotification;

public sealed class ResultCalculatedEventHandler : INotificationHandler<ResultCalculatedEventNotification>
{
    private readonly TriggerHostedService triggerHostedService;
    private readonly LeagueDbContext dbContext;

    public ResultCalculatedEventHandler(TriggerHostedService triggerHostedService, LeagueDbContext dbContext)
    {
        this.triggerHostedService = triggerHostedService;
        this.dbContext = dbContext;
    }

    public async Task Handle(ResultCalculatedEventNotification notification, CancellationToken cancellationToken)
    {
        var parameters = new TriggerParameterModel
        {
            EventType = TriggerEventType.ResultCalculated,
            EventId = notification.EventId,
        };
        await triggerHostedService.ProcessEventTriggers(dbContext, TriggerEventType.ResultCalculated, parameters, cancellationToken);
    }
}


