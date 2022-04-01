using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record GetScoringsFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<GetScoringModel>>;
    public class GetScoringsFromSeasonHandler : ScoringHandlerBase, IRequestHandler<GetScoringsFromSeasonRequest, IEnumerable<GetScoringModel>>
    {
        IEnumerable<IValidator<GetScoringsFromSeasonRequest>> validators;
        public GetScoringsFromSeasonHandler(LeagueDbContext dbContext, IEnumerable<IValidator<GetScoringsFromSeasonRequest>> validators) : base(dbContext)
        {
            this.validators = validators;
        }

        public async Task<IEnumerable<GetScoringModel>> Handle(GetScoringsFromSeasonRequest request,
            CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            return await MapToGetScoringModelFromSeason(request.LeagueId, request.SeasonId, cancellationToken);
        }

        private async Task<IEnumerable<GetScoringModel>> MapToGetScoringModelFromSeason(long leagueId, long seasonId,
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
