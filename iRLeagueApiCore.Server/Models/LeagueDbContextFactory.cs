using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace iRLeagueApiCore.Server.Models
{
    public class LeagueDbContextFactory : IDbContextFactory<LeagueDbContext>
    {
        private readonly IConfiguration _configuration;

        public LeagueDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public LeagueDbContext CreateDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
            optionsBuilder.UseMySQL(_configuration["Db:ConnectionString"]);

            var dbContext = new LeagueDbContext(optionsBuilder.Options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }
    }
}
