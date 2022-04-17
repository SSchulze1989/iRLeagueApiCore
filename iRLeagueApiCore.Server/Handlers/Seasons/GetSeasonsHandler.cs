using FluentValidation;
using iRLeagueApiCore.Communication.Models;
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
    public record GetSeasonsRequest(long LeagueId) : IRequest<IEnumerable<GetSeasonModel>>;

    public class GetSeasonsHandler : SeasonHandlerBase<GetSeasonsHandler, GetSeasonsRequest>, IRequestHandler<GetSeasonsRequest, IEnumerable<GetSeasonModel>>
    {
        public GetSeasonsHandler(ILogger<GetSeasonsHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSeasonsRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<GetSeasonModel>> Handle(GetSeasonsRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSeasons = await GetSeasonsEntityAsync(request.LeagueId, cancellationToken);
            if (getSeasons.Count() == 0)
            {
                throw new ResourceNotFoundException();
            }
            return getSeasons;
        }

        private async Task<IEnumerable<GetSeasonModel>> GetSeasonsEntityAsync(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Seasons
                .Where(x => x.LeagueId == leagueId)
                .Select(MapToGetSeasonModelExpression)
                .ToListAsync();
        }
    }
}
