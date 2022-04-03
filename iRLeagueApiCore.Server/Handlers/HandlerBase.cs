using FluentValidation;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers
{
    public abstract class HandlerBase<THandler, TRequest>
    {
        protected ILogger<THandler> _logger;
        protected LeagueDbContext dbContext;
        protected IEnumerable<IValidator<TRequest>> validators;

        protected HandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators)
        {
            _logger = logger;
            this.dbContext = dbContext;
            this.validators = validators;
        }

        protected virtual async Task<ScheduleEntity> GetScheduleEntityAsync(long leagueId, long? scheduleId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken);
        }

        protected virtual async Task<ScoringEntity> GetScoringEntityAsync(long leagueId, long? scoringId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.ScoringId == scoringId, cancellationToken);
        }

        protected virtual async Task<SeasonEntity> GetSeasonEntityAsync(long leagueId, long seasonId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Seasons
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SeasonId == seasonId, cancellationToken);
        }
    }
}
