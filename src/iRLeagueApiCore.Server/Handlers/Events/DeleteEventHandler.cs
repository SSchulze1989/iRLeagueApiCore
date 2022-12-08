using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Events
{
    public record DeleteEventRequest(long LeagueId, long EventId) : IRequest;

    public class DeleteEventHandler : EventHandlerBase<DeleteEventHandler, DeleteEventRequest>, IRequestHandler<DeleteEventRequest>
    {
        public DeleteEventHandler(ILogger<DeleteEventHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteEventRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteEventRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var deleteEvent = await GetEventEntityAsync(request.LeagueId, request.EventId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            dbContext.Events
                .Remove(deleteEvent);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
