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
    public record GetResultRequest(long LeagueId, long EventId, long ResultTabId) : IRequest<EventResultTabModel>;

    public class GetResultHandler : ResultHandlerBase<GetResultHandler, GetResultRequest>, IRequestHandler<GetResultRequest, EventResultTabModel>
    {
        public GetResultHandler(ILogger<GetResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<EventResultTabModel> Handle(GetResultRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getResult = await MapToEventResultModelAsync(request.LeagueId, request.EventId, request.ResultTabId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getResult;
        }

        private async Task<EventResultTabModel> MapToEventResultModelAsync(long leagueId, long eventId, long resultTabId, CancellationToken cancellationToken)
        {
            return await dbContext.ScoredEventResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.EventId == eventId)
                .Where(x => x.ResultTabId == resultTabId)
                .Select(MapToEventResultModelExpression)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
