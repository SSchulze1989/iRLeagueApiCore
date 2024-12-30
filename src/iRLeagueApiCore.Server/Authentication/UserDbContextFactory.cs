using Microsoft.IdentityModel.Protocols.Configuration;

namespace iRLeagueApiCore.Server.Authentication;

public sealed class UserDbContextFactory : IDbContextFactory<UserDbContext>
{
    private readonly IConfiguration _configuration;

    public UserDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public UserDbContext CreateDbContext()
    {
        var dbConnectionString = _configuration.GetConnectionString("UserDb") ??
            throw new InvalidConfigurationException("No connection string for 'UserDB' found in configuration");
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseMySQL(dbConnectionString);

        var dbContext = new UserDbContext(optionsBuilder.Options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
