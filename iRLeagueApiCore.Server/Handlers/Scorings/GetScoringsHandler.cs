using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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
    public record GetScoringsRequest(long LeagueId) : IRequest<IEnumerable<GetScoringModel>>;

    public class GetScoringsHandler : ScoringHandlerBase, IRequestHandler<GetScoringsRequest, IEnumerable<GetScoringModel>>
    {
        private readonly IEnumerable<IValidator<GetScoringsRequest>> validators;

        public GetScoringsHandler(LeagueDbContext dbContext, IEnumerable<IValidator<GetScoringsRequest>> validators)
            : base(dbContext)
        {
            this.validators = validators;
        }

        public async Task<IEnumerable<GetScoringModel>> Handle(GetScoringsRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            return await MapToGetScoringModelsAsync(request.LeagueId);
        }

        private async Task<IEnumerable<GetScoringModel>> MapToGetScoringModelsAsync(long leagueId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .Select(MapToGetScoringModelExpression)
                .ToListAsync();
        }
    }
}
