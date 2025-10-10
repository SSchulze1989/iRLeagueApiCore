using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.Services.TriggerService.Events;
public abstract class EventHandlerBase<T> : INotificationHandler<T> where T : INotification
{
    private readonly TriggerHostedService triggerHostedService;
    private readonly LeagueDbContext dbContext;

    protected EventHandlerBase(TriggerHostedService triggerHostedService, LeagueDbContext dbContext)
    {
        this.triggerHostedService = triggerHostedService;
        this.dbContext = dbContext;
    }

    protected abstract TriggerEventType TriggerEventType { get; }
    protected abstract Task<TriggerParameterModel> GetParameters(T notification, CancellationToken cancellationToken);

    public async Task Handle(T notification, CancellationToken cancellationToken)
    {
        var parameters = await GetParameters(notification, cancellationToken);
        await triggerHostedService.ProcessEventTriggers(dbContext, TriggerEventType, parameters, cancellationToken);
    }
}
