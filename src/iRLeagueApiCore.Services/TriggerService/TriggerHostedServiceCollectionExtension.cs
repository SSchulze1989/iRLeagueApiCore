using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.TriggerService;
using iRLeagueApiCore.Services.TriggerService.Actions;

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
        services.AddHostedService(x => new TriggerHostedService(
            x.GetRequiredService<ILogger<TriggerHostedService>>(),
            configuration,
            x.GetRequiredService<IServiceProvider>(),
            x.GetRequiredService<IBackgroundTaskQueue>()));
        return services;
    }
}
