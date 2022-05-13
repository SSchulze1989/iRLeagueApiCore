using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Schedules
{
    public record GetSchedulesFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<GetScheduleModel>>;

    public class GetSchedulesFromSeasonHandler : ScheduleHandlerBase<GetSchedulesFromSeasonRequest, GetSchedulesFromSeasonRequest>,
        IRequestHandler<GetSchedulesFromSeasonRequest, IEnumerable<GetScheduleModel>>
    {
        public GetSchedulesFromSeasonHandler(ILogger<GetSchedulesFromSeasonRequest> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSchedulesFromSeasonRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<GetScheduleModel>> Handle(GetSchedulesFromSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSchedules = await MapToGetScheduleModelsAsync(request.LeagueId, request.SeasonId, cancellationToken);
            return getSchedules;
        }

        private async Task<IEnumerable<GetScheduleModel>> MapToGetScheduleModelsAsync(long leagueId, long seasonId, CancellationToken cancellationToken)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SeasonId == seasonId)
                .Select(MapToGetScheduleModelExpression)
                .ToListAsync();
        }
    }
}
