using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Schedules
{
    public class ScheduleHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public ScheduleHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected virtual ScheduleEntity MapToScheduleEntity(LeagueUser user, PutScheduleModel postSchedule, ScheduleEntity target)
        {
            target.Name = postSchedule.Name;
            target.LastModifiedByUserId = user.Id;
            target.LastModifiedByUserName = user.Name;
            target.LastModifiedOn = DateTime.UtcNow;
            target.Version++;
            return target;
        }

        protected virtual async Task<GetScheduleModel> MapToGetScheduleModelAsync(long leagueId, long scheduleId, CancellationToken cancellationToken)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ScheduleId == scheduleId)
                .Select(MapToGetScheduleModelExpression)
                .SingleOrDefaultAsync(cancellationToken);
        }

        protected virtual Expression<Func<ScheduleEntity, GetScheduleModel>> MapToGetScheduleModelExpression => x => new GetScheduleModel()
        {
            LeagueId = x.LeagueId,
            ScheduleId = x.ScheduleId,
            SeasonId = x.SeasonId,
            Name = x.Name,
            SessionIds = x.Sessions.Select(x => x.SessionId),
            CreatedOn = x.CreatedOn,
            CreatedByUserId = x.CreatedByUserId,
            CreatedByUserName = x.CreatedByUserName,
            LastModifiedOn = x.LastModifiedOn,
            LastModifiedByUserId = x.LastModifiedByUserId,
            LastModifiedByUserName = x.LastModifiedByUserName
        };
    }
}
