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

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record ScoringAddSessionRequest(long LeagueId, long ScoringId, long SessionId) : IRequest;

    public class ScoringAddSessionHandler : ScoringHandlerBase<ScoringAddSessionHandler, ScoringAddSessionRequest>, IRequestHandler<ScoringAddSessionRequest>
    {
        public ScoringAddSessionHandler(ILogger<ScoringAddSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<ScoringAddSessionRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(ScoringAddSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var scoring = await GetScoringEntityAsync(request.LeagueId, request.ScoringId, cancellationToken) ?? throw new ResourceNotFoundException();
            await AddSessionToScoringAsync(request.LeagueId, scoring, request.SessionId, cancellationToken);
            await dbContext.SaveChangesAsync();
            return Unit.Value;
        }

        protected override async Task<ScoringEntity> GetScoringEntityAsync(long leagueId, long? scoringId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .Include(x => x.Sessions)
                .SingleOrDefaultAsync(x => x.ScoringId == scoringId, cancellationToken);
        }

        private async Task AddSessionToScoringAsync(long leagueId, ScoringEntity scoring, long sessionId, CancellationToken cancellationToken)
        {
            var session = await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SessionId == sessionId, cancellationToken) ?? throw new ResourceNotFoundException();
            if (scoring.Sessions.Contains(session) == false)
            {
                scoring.Sessions.Add(session);
            }
        }
    }
}
