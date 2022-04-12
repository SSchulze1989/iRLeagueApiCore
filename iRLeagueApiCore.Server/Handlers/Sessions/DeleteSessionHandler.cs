using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public record DeleteSessionRequest(long LeagueId, long SessionId) : IRequest;

    public class DeleteSessionHandler : SessionHandlerBase<DeleteSessionHandler, DeleteSessionRequest>, IRequestHandler<DeleteSessionRequest>
    {
        public DeleteSessionHandler(ILogger<DeleteSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteSessionRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var deleteSession = await GetSessionEntityAsync(request.LeagueId, request.SessionId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            dbContext.Sessions.Remove(deleteSession);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
