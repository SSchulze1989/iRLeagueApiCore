namespace iRLeagueApiCore.Server.Models;

public sealed class LeagueDbContextFactory : IDbContextFactory<LeagueDbContext>
{
    private readonly IConfiguration _configuration;

    public LeagueDbContextFactory(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public LeagueDbContext CreateDbContext()
    {
        var dbConnectionString = _configuration.GetConnectionString("ModelDb");
        var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
        optionsBuilder.UseMySQL(dbConnectionString);

        var dbContext = new LeagueDbContext(optionsBuilder.Options);
        dbContext.Database.Migrate();
        return dbContext;
    }
}
