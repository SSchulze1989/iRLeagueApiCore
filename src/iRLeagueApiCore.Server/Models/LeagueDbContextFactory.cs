using iRLeagueDatabaseCore;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace iRLeagueApiCore.Server.Models;

public sealed class LeagueDbContextFactory
{
    private readonly IConfiguration _configuration;

    public LeagueDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LeagueDbContext CreateDbContext(ILeagueProvider leagueProvider)
    {
        var dbConnectionString = _configuration.GetConnectionString("ModelDb") ??
            throw new InvalidConfigurationException("No connection string for 'ModelDb' found in configuration");
        var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
        optionsBuilder.UseMySQL(dbConnectionString);

        var dbContext = new LeagueDbContext(optionsBuilder.Options, leagueProvider);
        return dbContext;
    }
}
