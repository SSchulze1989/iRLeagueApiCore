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
    public record ScoringAddSessionRequest(long LeagueId, long ScoringId, long SessionId) : IRequest;

    public class ScoringAddSessionHandler : ScoringHandlerBase, IRequestHandler<ScoringAddSessionRequest>
    {
        private readonly IEnumerable<IValidator<ScoringAddSessionRequest>> validators;

        public ScoringAddSessionHandler(LeagueDbContext dbContext, IEnumerable<IValidator<ScoringAddSessionRequest>> validators)
            : base(dbContext)
        {
            this.validators = validators;
        }

        public async Task<Unit> Handle(ScoringAddSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var scoring = await GetScoringEntityAsync(request.LeagueId, request.ScoringId, cancellationToken) ?? throw new ResourceNotFoundException();
            await AddSessionToScoringAsync(request.LeagueId, scoring, request.SessionId, cancellationToken);
            await dbContext.SaveChangesAsync();
            return Unit.Value;
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
