using Microsoft.IdentityModel.Protocols.Configuration;

namespace iRLeagueApiCore.Server.Authentication;

public sealed class UserDbContextFactory : IDbContextFactory<UserDbContext>
{
    private readonly DbContextOptions<UserDbContext> contextOptions;

    public UserDbContextFactory(IConfiguration configuration)
    {
        // load the database configurations once at start - reloading is not enabled
        var dbConnectionString = configuration.GetConnectionString("UserDb") ??
            throw new InvalidConfigurationException("No connection string for 'UserDB' found in configuration");
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseMySQL(dbConnectionString);
        contextOptions = optionsBuilder.Options;
    }

    public UserDbContext CreateDbContext()
    {
        var dbContext = new UserDbContext(contextOptions);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
