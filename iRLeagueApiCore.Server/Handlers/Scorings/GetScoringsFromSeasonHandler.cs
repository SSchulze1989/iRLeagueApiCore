using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record GetScoringsFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<ScoringModel>>;
    public class GetScoringsFromSeasonHandler : ScoringHandlerBase<GetScoringsFromSeasonHandler, GetScoringsFromSeasonRequest>, 
        IRequestHandler<GetScoringsFromSeasonRequest, IEnumerable<ScoringModel>>
    {
        public GetScoringsFromSeasonHandler(ILogger<GetScoringsFromSeasonHandler> logger, LeagueDbContext dbContext, 
            IEnumerable<IValidator<GetScoringsFromSeasonRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<ScoringModel>> Handle(GetScoringsFromSeasonRequest request,
            CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            return await MapToGetScoringModelFromSeason(request.LeagueId, request.SeasonId, cancellationToken);
        }

        private async Task<IEnumerable<ScoringModel>> MapToGetScoringModelFromSeason(long leagueId, long seasonId,
            CancellationToken cancellationToken = default)
        {
            var seasonScorings = await dbContext.Seasons
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SeasonId == seasonId)
                .Select(x => new
                {
                    SeasonId = x.SeasonId,
                    Scorings = x.Scorings
                        .AsQueryable()
                        .Select(MapToGetScoringModelExpression)
                        .ToList()
                })
                .SingleOrDefaultAsync(cancellationToken) ?? throw new ResourceNotFoundException();
            return seasonScorings.Scorings;
        }
    }
}
