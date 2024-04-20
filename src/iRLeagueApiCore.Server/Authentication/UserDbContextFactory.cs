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
        var dbConnectionString = _configuration.GetConnectionString("UserDb")
            ?? throw new KeyNotFoundException("No connection string was registered for \"UserDb\"");
        var optionsBuilder = new DbContextOptionsBuilder<UserDbContext>();
        optionsBuilder.UseMySQL(dbConnectionString);

        var dbContext = new UserDbContext(optionsBuilder.Options);
        dbContext.Database.EnsureCreated();
        return dbContext;
    }
}
