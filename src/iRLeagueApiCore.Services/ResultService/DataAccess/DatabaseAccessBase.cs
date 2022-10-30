using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal abstract class DatabaseAccessBase
    {
        protected readonly ILeagueDbContext dbContext;

        public DatabaseAccessBase(ILeagueDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
