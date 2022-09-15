using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public record DeleteSeasonRequest(long LeagueId, long SeasonId) : IRequest;

    public class DeleteSeasonHandler : SeasonHandlerBase<DeleteSeasonHandler, DeleteSeasonRequest>, IRequestHandler<DeleteSeasonRequest>
    {
        public DeleteSeasonHandler(ILogger<DeleteSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteSeasonRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            await DeleteSeasonEntity(request.LeagueId, request.SeasonId, cancellationToken);
            await dbContext.SaveChangesAsync();
            return Unit.Value;
        }

        protected async Task DeleteSeasonEntity(long leagueId, long seasonId, CancellationToken cancellationToken)
        {
            var deleteSeason = await dbContext.Seasons
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SeasonId == seasonId) 
                ?? throw new ResourceNotFoundException();
            dbContext.Seasons.Remove(deleteSeason);
        }
    }
}
