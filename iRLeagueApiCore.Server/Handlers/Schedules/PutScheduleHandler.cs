using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Schedules
{
    public record PutScheduleRequest(long LeagueId, LeagueUser User, long ScheduleId, PutScheduleModel Model) : IRequest<GetScheduleModel>;

    public class PutScheduleHandler : ScheduleHandlerBase<PutScheduleHandler, PutScheduleRequest>,
        IRequestHandler<PutScheduleRequest, GetScheduleModel>
    {
        public PutScheduleHandler(ILogger<PutScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutScheduleRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<GetScheduleModel> Handle(PutScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putSchedule = await GetScheduleEntityAsync(request.LeagueId, request.ScheduleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            putSchedule = MapToScheduleEntity(request.User, request.Model, putSchedule);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getSchedule = await MapToGetScheduleModelAsync(request.LeagueId, request.ScheduleId, cancellationToken);
            return getSchedule;
        }
    }
}
