using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal abstract class DatabaseAccessBase
    {
        protected readonly LeagueDbContext dbContext;

        public DatabaseAccessBase(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
    }
}
