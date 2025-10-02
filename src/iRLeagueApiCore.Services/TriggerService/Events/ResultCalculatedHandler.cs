using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.Services.TriggerService.Events;
public record ResultCalculatedNotification(long EventId) : INotification;

public sealed class ResultCalculatedHandler : INotificationHandler<ResultCalculatedNotification>
{
    private readonly TriggerHostedService triggerHostedService;
    private readonly LeagueDbContext dbContext;

    public ResultCalculatedHandler(TriggerHostedService triggerHostedService, LeagueDbContext dbContext)
    {
        this.triggerHostedService = triggerHostedService;
        this.dbContext = dbContext;
    }

    public async Task Handle(ResultCalculatedNotification notification, CancellationToken cancellationToken)
    {
        var parameters = new TriggerParameterModel
        {
            EventType = TriggerEventType.ResultCalculated,
            EventId = notification.EventId,
        };
        await triggerHostedService.ProcessEventTriggers(dbContext, TriggerEventType.ResultCalculated, parameters, cancellationToken);
    }
}


