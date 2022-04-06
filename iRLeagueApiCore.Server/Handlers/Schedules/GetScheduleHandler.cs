using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Schedules
{
    public record GetScheduleRequest(long LeagueId, long ScheduleId) : IRequest<GetScheduleModel>;

    public class GetScheduleHandler : ScheduleHandlerBase<GetScheduleHandler, GetScheduleRequest>,
        IRequestHandler<GetScheduleRequest, GetScheduleModel>
    {
        public GetScheduleHandler(ILogger<GetScheduleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetScheduleRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<GetScheduleModel> Handle(GetScheduleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSchedule = await MapToGetScheduleModelAsync(request.LeagueId, request.ScheduleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getSchedule;
        }
    }
}
