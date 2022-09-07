using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Events
{
    public record GetEventRequest(long LeagueId, long EventId) : IRequest<EventModel>;

    public class GetEventHandler : EventHandlerBase<GetEventHandler, GetEventRequest>, IRequestHandler<GetEventRequest, EventModel>
    {
        public GetEventHandler(ILogger<GetEventHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetEventRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<EventModel> Handle(GetEventRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getEvent = await MapToEventModelAsync(request.LeagueId, request.EventId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getEvent;
        }
    }
}
