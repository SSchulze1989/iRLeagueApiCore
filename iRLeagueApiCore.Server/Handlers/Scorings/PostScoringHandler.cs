using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record PostScoringRequest(long LeagueId, long SeasonId, PostScoringModel Model) : IRequest<GetScoringModel>;

    public class PostScoringHandler : ScoringHandlerBase<PostScoringHandler, PostScoringRequest>, IRequestHandler<PostScoringRequest, GetScoringModel>
    {
        public PostScoringHandler(ILogger<PostScoringHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostScoringRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<GetScoringModel> Handle(PostScoringRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            _logger.LogInformation("Creating scoring for league {LeagueId} in season {SeasonId}",
                request.LeagueId, request.SeasonId);
            var postScoring = await CreateScoringEntityAsync(request.LeagueId, request.SeasonId);
            postScoring = await MapToScoringEntityAsync(request.LeagueId, request.Model, postScoring);
            dbContext.SaveChanges();
            _logger.LogInformation("Scoring created successfully => scoring id: {ScoringId}", postScoring.ScoringId);
            Debug.Assert(postScoring.ScoringId != default(long));
            var getScoring = await MapToGetScoringModelAsync(request.LeagueId, postScoring.ScoringId);
            return getScoring;
        }

        private async Task<ScoringEntity> CreateScoringEntityAsync(long leagueId, long seasonId)
        {
            var scoring = new ScoringEntity();
            scoring.LeagueId = leagueId;
            var season = await GetSeasonEntityAsync(leagueId, seasonId) ?? throw new ResourceNotFoundException();
            season.Scorings.Add(scoring);
            return scoring;
        }
    }
}
