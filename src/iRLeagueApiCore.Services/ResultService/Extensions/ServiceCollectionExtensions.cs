using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace iRLeagueApiCore.Services.ResultService.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddResultService(this IServiceCollection services)
        {
            services.TryAddScoped<ILeagueDbContext>(s => s.GetRequiredService<LeagueDbContext>());
            return services;
        }
    }
}
