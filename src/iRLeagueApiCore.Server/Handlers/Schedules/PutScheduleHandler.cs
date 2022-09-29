using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Schedules
{
    public record PutScheduleRequest(long LeagueId, LeagueUser User, long ScheduleId, PutScheduleModel Model) : IRequest<ScheduleModel>;

    public class PutScheduleHandler : ScheduleHandlerBase<PutScheduleHandler, PutScheduleRequest>,
        IRequestHandler<PutScheduleRequest, ScheduleModel>
    {
        public PutScheduleHandler(ILogger<PutScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutScheduleRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<ScheduleModel> Handle(PutScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putSchedule = await GetScheduleEntityAsync(request.LeagueId, request.ScheduleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            putSchedule = MapToScheduleEntity(request.User, request.Model, putSchedule);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getSchedule = await MapToGetScheduleModelAsync(request.LeagueId, request.ScheduleId, cancellationToken)
                ?? throw new InvalidOperationException("Created resource was not found");
            return getSchedule;
        }
    }
}
