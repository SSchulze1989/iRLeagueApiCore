using Microsoft.Extensions.DependencyInjection;

namespace iRLeagueApiCore.Services.Data.DriverPoints;

public static class DriverPointsRepositoryExtensions
{
    public static IServiceCollection AddDriverPointsData(this IServiceCollection services)
    {
        services.AddScoped<IDriverPointsRepository, DriverPointsRepository>();
        return services;
    }
}
