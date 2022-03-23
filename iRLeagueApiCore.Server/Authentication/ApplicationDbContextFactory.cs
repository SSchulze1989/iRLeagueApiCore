using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace iRLeagueApiCore.Server.Authentication
{
    public class ApplicationDbContextFactory : IDbContextFactory<ApplicationDbContext>
    {
        private readonly IConfiguration _configuration;

        public ApplicationDbContextFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public ApplicationDbContext CreateDbContext()
        {
            var dbConnectionString = _configuration.GetConnectionString("UserDb");
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
            optionsBuilder.UseMySQL(dbConnectionString);

            var dbContext = new ApplicationDbContext(optionsBuilder.Options);
            dbContext.Database.EnsureCreated();
            return dbContext;
        }
    }
}
