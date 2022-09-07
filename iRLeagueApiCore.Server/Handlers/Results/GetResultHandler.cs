using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
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
    public record GetResultRequest(long LeagueId, long ResultId) : IRequest<EventResultModel>;

    public class GetResultHandler : ResultHandlerBase<GetResultHandler, GetResultRequest>, IRequestHandler<GetResultRequest, EventResultModel>
    {
        public GetResultHandler(ILogger<GetResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<EventResultModel> Handle(GetResultRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getResult = await MapToEventResultModelAsync(request.LeagueId, request.ResultId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getResult;
        }

        private async Task<EventResultModel> MapToEventResultModelAsync(long leagueId, long resultId, CancellationToken cancellationToken)
        {
            return await dbContext.ScoredEventResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ResultId == resultId)
                .Select(MapToEventResultModelExpression)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
