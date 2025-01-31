using iRLeagueDatabaseCore;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace iRLeagueApiCore.Server.Models;

public sealed class LeagueDbContextFactory
{
    private readonly bool applyMigrations = false;
    private readonly DbContextOptions<LeagueDbContext> contextOptions;
    

    public LeagueDbContextFactory(IConfiguration configuration)
    {
        applyMigrations = configuration.GetValue<bool>("ApplyDatabaseMigrations") == true;

        // load the database configurations once at start - reloading is not enabled
        var dbConnectionString = configuration.GetConnectionString("ModelDb") ??
            throw new InvalidConfigurationException("No connection string for 'ModelDb' found in configuration");
        var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
        optionsBuilder.UseMySQL(dbConnectionString);
        contextOptions = optionsBuilder.Options;
    }

    public LeagueDbContext CreateDbContext(ILeagueProvider leagueProvider)
    {
        var dbContext = new LeagueDbContext(contextOptions, leagueProvider);
        if (applyMigrations)
        {
            dbContext.Database.Migrate();
        }
        return dbContext;
    }
}
