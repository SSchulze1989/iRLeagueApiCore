using iRLeagueApiCore.Services.TriggerService;

namespace Microsoft.Extensions.DependencyInjection;
public static class TriggerHostedServiceCollectionExtension
{
    public static IServiceCollection AddTriggerService(this IServiceCollection services, Action<TriggerHostedServiceConfiguration>? configure = null)
    {
        var configuration = configure is not null ? new TriggerHostedServiceConfiguration() : new();
        services.AddHostedService(x => new TriggerHostedService(x.GetRequiredService<ILogger<TriggerHostedService>>(), configuration, x.GetRequiredService<IServiceProvider>()));
        return services;
    }
}
