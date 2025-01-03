using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Excecution;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection;

public static class ResultServiceCollectionExtensions
{
    public static IServiceCollection AddResultService(this IServiceCollection services)
    {
        // Result calculation
        services.TryAddScoped<ILeagueDbContext>(s => s.GetRequiredService<LeagueDbContext>());
        services.TryAddScoped<ICalculationServiceProvider<EventCalculationConfiguration, EventCalculationData, EventCalculationResult>>(
            x => new EventCalculationServiceProvider(
                x.GetRequiredService<ICalculationServiceProvider<SessionCalculationConfiguration, SessionCalculationData, SessionCalculationResult>>()));
        services.TryAddScoped<ICalculationServiceProvider<SessionCalculationConfiguration, SessionCalculationData, SessionCalculationResult>>(
            x => new SessionCalculationServiceProvider());
        services.TryAddScoped<IEventCalculationConfigurationProvider>(x => new EventCalculationConfigurationProvider(
            x.GetRequiredService<LeagueDbContext>(),
            x.GetRequiredService<ISessionCalculationConfigurationProvider>()));
        services.TryAddScoped<IEventCalculationDataProvider>(x => new EventCalculationDataProvider(
            x.GetRequiredService<LeagueDbContext>()));
        services.TryAddScoped<IEventCalculationResultStore>(x => new EventCalculationResultStore(
            x.GetRequiredService<LeagueDbContext>()));
        services.TryAddScoped<ISessionCalculationConfigurationProvider>(x => new SessionCalculationConfigurationProvider(
            x.GetRequiredService<LeagueDbContext>()));
        services.TryAddScoped(x => new ExecuteEventResultCalculation(
            logger: x.GetRequiredService<ILogger<ExecuteEventResultCalculation>>(),
            dataProvider: x.GetRequiredService<IEventCalculationDataProvider>(),
            configProvider: x.GetRequiredService<IEventCalculationConfigurationProvider>(),
            dataStore: x.GetRequiredService<IEventCalculationResultStore>(),
            calculationServiceProvider: x.GetRequiredService<ICalculationServiceProvider<EventCalculationConfiguration, EventCalculationData, EventCalculationResult>>(),
            standingCalculationQueue: x.GetRequiredService<IStandingCalculationQueue>()));
        services.TryAddScoped<IResultCalculationQueue>(x => new ResultCalculationQueue(x, x.GetRequiredService<IBackgroundTaskQueue>()));
        // Standing calculation
        services.TryAddScoped<ICalculationServiceProvider<StandingCalculationConfiguration, StandingCalculationData, StandingCalculationResult>>(
            x => new StandingCalculationServiceProvider());
        services.TryAddScoped<IStandingCalculationConfigurationProvider>(x => new StandingCalculationConfigurationProvider(
            x.GetRequiredService<LeagueDbContext>()));
        services.TryAddScoped<IStandingCalculationDataProvider>(x => new StandingCalculationDataProvider(
            x.GetRequiredService<LeagueDbContext>()));
        services.TryAddScoped<IStandingCalculationResultStore>(x => new StandingCalculationResultStore(
            x.GetRequiredService<LeagueDbContext>()));
        services.TryAddScoped(x => new ExecuteStandingCalculation(
            logger: x.GetRequiredService<ILogger<ExecuteStandingCalculation>>(),
            dataProvider: x.GetRequiredService<IStandingCalculationDataProvider>(),
            configProvider: x.GetRequiredService<IStandingCalculationConfigurationProvider>(),
            dataStore: x.GetRequiredService<IStandingCalculationResultStore>(),
            calculationServiceProvider: x.GetRequiredService<ICalculationServiceProvider<StandingCalculationConfiguration, StandingCalculationData, StandingCalculationResult>>(),
            standingCalculationQueue: x.GetRequiredService<IStandingCalculationQueue>()));
        services.TryAddScoped<IStandingCalculationQueue>(x => new StandingCalculationQueue(x, x.GetRequiredService<IBackgroundTaskQueue>()));

        return services;
    }
}
