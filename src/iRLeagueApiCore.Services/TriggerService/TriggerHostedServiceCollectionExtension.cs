using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.TriggerService;
using iRLeagueApiCore.Services.TriggerService.Actions;
using iRLeagueApiCore.Services.TriggerService.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;
public static class TriggerHostedServiceCollectionExtension
{
    public static IServiceCollection AddTriggerService(this IServiceCollection services, Action<TriggerHostedServiceConfiguration>? configure = null)
    {
        var configuration = new TriggerHostedServiceConfiguration();
        if (configure is not null)
        {
            configure(configuration);
        }
        services.TryAddScoped<TriggerActionProvider>();
        services.TryAddSingleton(x => new TriggerHostedService(
            x.GetRequiredService<ILogger<TriggerHostedService>>(),
            configuration,
            x.GetRequiredService<IServiceProvider>(),
            x.GetRequiredService<IBackgroundTaskQueue>()));
        services.AddHostedService(sp => sp.GetRequiredService<TriggerHostedService>());

        // Event notification handlers
        services.TryAddScoped<INotificationHandler<ResultCalculatedEventNotification>, ResultCalculatedEventHandler>();
        services.TryAddScoped<INotificationHandler<StandingsUpdatedEventNotification>, StandingsUpdatedEventHandler>();
        return services;
    }

    public static IServiceCollection AddTriggerAction<TAction>(this IServiceCollection services, TriggerAction key) where TAction : class, ITriggerAction
    {
        services.TryAddKeyedTransient<ITriggerAction, TAction>(key);
        return services;
    }
}
