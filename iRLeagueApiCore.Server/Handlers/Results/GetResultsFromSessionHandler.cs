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

namespace iRLeagueApiCore.Server.Handlers.Results
{
    public record GetResultsFromSessionRequest(long LeagueId, long SessionId) : IRequest<IEnumerable<ResultModel>>;

    public class GetResultsFromSessionHandler : ResultHandlerBase<GetResultsFromSessionHandler, GetResultsFromSessionRequest>, 
        IRequestHandler<GetResultsFromSessionRequest, IEnumerable<ResultModel>>
    {
        public GetResultsFromSessionHandler(ILogger<GetResultsFromSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultsFromSessionRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<ResultModel>> Handle(GetResultsFromSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getResults = await MapToGetResultModelsFromSessionAsync(request.LeagueId, request.SessionId, cancellationToken);
            if (getResults.Count() == 0)
            {
                throw new ResourceNotFoundException();
            }
            return getResults;
        }

        private async Task<IEnumerable<ResultModel>> MapToGetResultModelsFromSessionAsync(long leagueId, long sessionId, CancellationToken cancellationToken)
        {
            return await dbContext.ScoredResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.Result.Session.SessionId == sessionId)
                .Select(MapToGetResultModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
