using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record DeleteScoringRequest(long LeagueId, long ScoringId) : IRequest;

    public class DeleteScoringHandler : ScoringHandlerBase<DeleteScoringHandler, DeleteScoringRequest>, IRequestHandler<DeleteScoringRequest>
    {
        public DeleteScoringHandler(ILogger<DeleteScoringHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteScoringRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteScoringRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var scoring = await GetScoringEntityAsync(request.LeagueId, request.ScoringId) ?? throw new ResourceNotFoundException();
            var scoredResults = dbContext.ScoredResults.ToList();
            dbContext.Scorings.Remove(scoring);
            await dbContext.SaveChangesAsync();
            _logger.LogInformation("Removed scoring {ScoringId} inside league {LeagueId} from database", 
                scoring.ScoringId, scoring.LeagueId);
            return Unit.Value;
        }
    }
}
