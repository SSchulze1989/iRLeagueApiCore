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
    public record GetResultsFromEventRequest(long LeagueId, long EventId) : IRequest<IEnumerable<EventResultTabModel>>;

    public class GetResultsFromSessionHandler : ResultHandlerBase<GetResultsFromSessionHandler, GetResultsFromEventRequest>, 
        IRequestHandler<GetResultsFromEventRequest, IEnumerable<EventResultTabModel>>
    {
        public GetResultsFromSessionHandler(ILogger<GetResultsFromSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultsFromEventRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<EventResultTabModel>> Handle(GetResultsFromEventRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getResults = await MapToGetResultModelsFromSessionAsync(request.LeagueId, request.EventId, cancellationToken);
            if (getResults.Count() == 0)
            {
                throw new ResourceNotFoundException();
            }
            return getResults;
        }

        private async Task<IEnumerable<EventResultTabModel>> MapToGetResultModelsFromSessionAsync(long leagueId, long eventId, CancellationToken cancellationToken)
        {
            return await dbContext.ScoredEventResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Select(MapToEventResultModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
