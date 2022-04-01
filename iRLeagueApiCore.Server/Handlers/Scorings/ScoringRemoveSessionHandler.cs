using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record ScoringRemoveSessionRequest(long LeagueId, long ScoringId, long SessionId) : IRequest;

    public class ScoringRemoveSessionHandler : ScoringHandlerBase, IRequestHandler<ScoringRemoveSessionRequest>
    {
        private readonly IEnumerable<IValidator<ScoringRemoveSessionRequest>> validators;

        public ScoringRemoveSessionHandler(LeagueDbContext dbContext, IEnumerable<IValidator<ScoringRemoveSessionRequest>> validators) 
            : base(dbContext)
        {
            this.validators = validators;
        }

        public async Task<Unit> Handle(ScoringRemoveSessionRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var scoring = await GetScoringEntityAsync(request.LeagueId, request.ScoringId, cancellationToken) ?? throw new ResourceNotFoundException();
            await RemoveSessionFromScoringAsync(request.LeagueId, scoring, request.SessionId);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }

        protected override async Task<ScoringEntity> GetScoringEntityAsync(long leagueId, long? scoringId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .Include(x => x.Sessions)
                .SingleOrDefaultAsync(x => x.ScoringId == scoringId, cancellationToken);
        }

        private async Task RemoveSessionFromScoringAsync(long leagueId, ScoringEntity scoring, long sessionId)
        {
            var session = await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SessionId == sessionId) ?? throw new ResourceNotFoundException();
            if (scoring.Sessions.Contains(session))
            {
                scoring.Sessions.Remove(session);
            }
        }
    }
}
