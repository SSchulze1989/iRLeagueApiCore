using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Schedules
{
    public record GetSchedulesFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<ScheduleModel>>;

    public class GetSchedulesFromSeasonHandler : ScheduleHandlerBase<GetSchedulesFromSeasonRequest, GetSchedulesFromSeasonRequest>,
        IRequestHandler<GetSchedulesFromSeasonRequest, IEnumerable<ScheduleModel>>
    {
        public GetSchedulesFromSeasonHandler(ILogger<GetSchedulesFromSeasonRequest> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSchedulesFromSeasonRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<ScheduleModel>> Handle(GetSchedulesFromSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSchedules = await MapToGetScheduleModelsAsync(request.LeagueId, request.SeasonId, cancellationToken);
            return getSchedules;
        }

        private async Task<IEnumerable<ScheduleModel>> MapToGetScheduleModelsAsync(long leagueId, long seasonId, CancellationToken cancellationToken)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SeasonId == seasonId)
                .Select(MapToGetScheduleModelExpression)
                .ToListAsync();
        }
    }
}
