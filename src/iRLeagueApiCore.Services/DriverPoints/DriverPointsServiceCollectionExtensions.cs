using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using iRLeagueApiCore.Services.Data.DriverPoints;
using iRLeagueApiCore.Services.DriverPoints;

namespace Microsoft.Extensions.DependencyInjection;

public static class DriverPointsServiceCollectionExtensions
{
    public static IServiceCollection AddDriverPoints(this IServiceCollection services)
    {
        // register repository and service
        services.TryAddScoped<IDriverPointsRepository, DriverPointsRepository>();
        services.TryAddScoped<IDriverPointsService, DriverPointsService>();
        return services;
    }
}
