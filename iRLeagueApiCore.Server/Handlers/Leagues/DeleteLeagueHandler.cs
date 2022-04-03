using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public record DeleteLeagueRequest(long LeagueId) : IRequest;

    public class DeleteLeagueHandler : LeagueHandlerBase<DeleteLeagueHandler, DeleteLeagueRequest>, IRequestHandler<DeleteLeagueRequest>
    {
        public DeleteLeagueHandler(ILogger<DeleteLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteLeagueRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteLeagueRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            _logger.LogInformation("Deleted league id {LeagueId}", request.LeagueId);
            var deleteLeague = await GetLeagueEntityAsync(request.LeagueId) ?? throw new ResourceNotFoundException();
            dbContext.Remove(deleteLeague);
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Deleted league {LeagueName} with league id {LeagueId} successfully", deleteLeague.Name, deleteLeague.Id);
            return Unit.Value;
        }
    }
}
