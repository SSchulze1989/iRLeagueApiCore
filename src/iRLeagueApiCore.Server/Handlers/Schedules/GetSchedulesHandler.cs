using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
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
    public record GetSchedulesRequest(long LeagueId) : IRequest<IEnumerable<ScheduleModel>>;

    public class GetSchedulesHandler : ScheduleHandlerBase<GetSchedulesHandler, GetSchedulesRequest>, IRequestHandler<GetSchedulesRequest, IEnumerable<ScheduleModel>>
    {
        public GetSchedulesHandler(ILogger<GetSchedulesHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSchedulesRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<ScheduleModel>> Handle(GetSchedulesRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSchedules = await MapToGetScheduleModelsAsync(request.LeagueId, cancellationToken);
            if (getSchedules.Count() == 0)
            {
                throw new ResourceNotFoundException();
            }
            return getSchedules;
        }

        protected virtual async Task<IEnumerable<ScheduleModel>> MapToGetScheduleModelsAsync(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .Select(MapToGetScheduleModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
