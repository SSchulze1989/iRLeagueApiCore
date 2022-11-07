using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Excecution;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ResultServiceCollectionExtensions
    {
        public static IServiceCollection AddResultService(this IServiceCollection services)
        {
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
                x.GetRequiredService<ILogger<ExecuteEventResultCalculation>>(),
                x.GetRequiredService<IEventCalculationDataProvider>(),
                x.GetRequiredService<IEventCalculationConfigurationProvider>(),
                x.GetRequiredService<IEventCalculationResultStore>(),
                x.GetRequiredService<ICalculationServiceProvider<EventCalculationConfiguration, EventCalculationData, EventCalculationResult>>()));

            return services;
        }
    }
}
