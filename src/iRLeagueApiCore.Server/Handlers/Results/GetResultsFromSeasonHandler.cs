using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Results
{
    public record GetResultsFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<EventResultModel>>;

    public class GetResultsFromSeasonHandler : ResultHandlerBase<GetResultsFromSeasonHandler, GetResultsFromSeasonRequest>, IRequestHandler<GetResultsFromSeasonRequest, IEnumerable<EventResultModel>>
    {
        public GetResultsFromSeasonHandler(ILogger<GetResultsFromSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultsFromSeasonRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<EventResultModel>> Handle(GetResultsFromSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getResults = await MapToGetResultModelsFromSeasonAsync(request.LeagueId, request.SeasonId, cancellationToken);
            if (getResults.Count() == 0)
            {
                throw new ResourceNotFoundException();
            }
            return getResults;
        }

        private async Task<IEnumerable<EventResultModel>> MapToGetResultModelsFromSeasonAsync(long leagueId, long seasonId, CancellationToken cancellationToken)
        {
            return await dbContext.ScoredEventResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.Event.Schedule.SeasonId == seasonId)
                .Select(MapToEventResultModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
