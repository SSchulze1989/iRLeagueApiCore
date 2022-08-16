using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Schedules
{
    public record PostScheduleRequest(long LeagueId, long seasonId, LeagueUser User, PostScheduleModel Model) : IRequest<ScheduleModel>;

    public class PostScheduleHandler : ScheduleHandlerBase<PostScheduleHandler, PostScheduleRequest>,
        IRequestHandler<PostScheduleRequest, ScheduleModel>
    {
        public PostScheduleHandler(ILogger<PostScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostScheduleRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<ScheduleModel> Handle(PostScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var postSchedule = await CreateScheduleEntityAsync(request.LeagueId, request.seasonId, request.User, cancellationToken);
            postSchedule = MapToScheduleEntity(request.User, request.Model, postSchedule);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getSchedule = await MapToGetScheduleModelAsync(request.LeagueId, postSchedule.ScheduleId, cancellationToken);
            return getSchedule;
        }

        private async Task<ScheduleEntity> CreateScheduleEntityAsync(long leagueId, long seasonId, LeagueUser user, CancellationToken cancellationToken)
        {
            var scheduleEntity = new ScheduleEntity()
            {
                CreatedByUserId = user.Id,
                CreatedByUserName = user.Name,
                CreatedOn = DateTime.UtcNow
            };
            var season = await dbContext.Seasons
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SeasonId == seasonId)
                ?? throw new ResourceNotFoundException();
            season.Schedules.Add(scheduleEntity);
            return scheduleEntity;
        }
    }
}
