using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.TriggerService;
using iRLeagueApiCore.Services.TriggerService.Actions;
using iRLeagueApiCore.Services.TriggerService.Events;
using MediatR;

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
        services.AddKeyedTransient<ITriggerAction, WebhookTriggerAction>(TriggerAction.Webhook);
        services.AddScoped<TriggerActionProvider>();
        services.AddSingleton(x => new TriggerHostedService(
            x.GetRequiredService<ILogger<TriggerHostedService>>(),
            configuration,
            x.GetRequiredService<IServiceProvider>(),
            x.GetRequiredService<IBackgroundTaskQueue>()));
        services.AddHostedService(sp => sp.GetRequiredService<TriggerHostedService>());

        // Event notification handlers
        services.AddScoped<INotificationHandler<ResultCalculatedNotification>, ResultCalculatedHandler>();
        return services;
    }
}
