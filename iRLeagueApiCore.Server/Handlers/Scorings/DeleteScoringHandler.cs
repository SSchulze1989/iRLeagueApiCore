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

    public class DeleteScoringHandler : ScoringHandlerBase, IRequestHandler<DeleteScoringRequest>
    {
        private readonly ILogger<DeleteScoringHandler> _logger;
        private readonly IEnumerable<IValidator<DeleteScoringRequest>> validators;

        public DeleteScoringHandler(ILogger<DeleteScoringHandler> logger, IEnumerable<IValidator<DeleteScoringRequest>> validators,
            LeagueDbContext dbContext) : base(dbContext)
        {
            _logger = logger;
            this.validators = validators;
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
