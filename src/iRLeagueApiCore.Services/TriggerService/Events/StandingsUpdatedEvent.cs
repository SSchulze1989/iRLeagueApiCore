using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.Services.TriggerService.Events;

public record StandingsUpdatedEventNotification(long seasonId) : INotification;

public sealed class StandingsUpdatedEventHandler : EventHandlerBase<StandingsUpdatedEventNotification>
{
    public StandingsUpdatedEventHandler(TriggerHostedService triggerHostedService, LeagueDbContext dbContext) :
        base(triggerHostedService, dbContext)
    {
    }

    protected override TriggerEventType TriggerEventType => TriggerEventType.StandingsUpdated;

    protected override Task<TriggerParameterModel> GetParameters(StandingsUpdatedEventNotification notification, CancellationToken cancellationToken)
    {
        var parameters = new TriggerParameterModel()
        {
            EventType = TriggerEventType.StandingsUpdated,
            RefId1 = notification.seasonId,
        };
        return Task.FromResult(parameters);
    }
}
