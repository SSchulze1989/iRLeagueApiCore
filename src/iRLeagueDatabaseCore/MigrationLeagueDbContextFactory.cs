using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore.Design;
using Moq;

namespace iRLeagueDatabaseCore;

internal class MigrationLeagueDbContextFactory : IDesignTimeDbContextFactory<LeagueDbContext>
{
    public LeagueDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("EFCORETOOLSDB");
        var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
        if (string.IsNullOrEmpty(connectionString))
        {
            optionsBuilder.UseMySQL();
        }
        else
        {
            optionsBuilder.UseMySQL(connectionString);
        }
        var leagueProvider = Mock.Of<ILeagueProvider>();

        var dbContext = new LeagueDbContext(optionsBuilder.Options, leagueProvider);
        return dbContext;
    }
}
