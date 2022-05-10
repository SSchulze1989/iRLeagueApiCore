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

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public record GetSessionsFromScheduleRequest(long LeagueId, long ScheduleId) : IRequest<IEnumerable<GetSessionModel>>;

    public class GetSessionsFromScheduleHandler : SessionHandlerBase<GetSessionsFromScheduleHandler, GetSessionsFromScheduleRequest>,
        IRequestHandler<GetSessionsFromScheduleRequest, IEnumerable<GetSessionModel>>
    {
        public GetSessionsFromScheduleHandler(ILogger<GetSessionsFromScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSessionsFromScheduleRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<GetSessionModel>> Handle(GetSessionsFromScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSessions = await MapToGetSessionModelsAsync(request.LeagueId, request.ScheduleId, cancellationToken);
            return getSessions;
        }

        private async Task<IEnumerable<GetSessionModel>> MapToGetSessionModelsAsync(long leagueId, long scheduleId, CancellationToken cancellationToken)
        {
            return await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ScheduleId == scheduleId)
                .Select(MapToGetSessionModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
