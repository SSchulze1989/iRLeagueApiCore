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
    public record GetResultRequest(long LeagueId, long SessionId, long ScoringId) : IRequest<ResultModel>;

    public class GetResultHandler : ResultHandlerBase<GetResultHandler, GetResultRequest>, IRequestHandler<GetResultRequest, ResultModel>
    {
        public GetResultHandler(ILogger<GetResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ResultModel> Handle(GetResultRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getResult = await MapToGetResultModelAsync(request.LeagueId, request.SessionId, request.ScoringId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getResult;
        }

        private async Task<ResultModel> MapToGetResultModelAsync(long leagueId, long sessionId, long ScoringId, CancellationToken cancellationToken)
        {
            return await dbContext.ScoredResults
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ResultId == sessionId)
                .Where(x => x.ScoringId == ScoringId)
                .Select(MapToGetResultModelExpression)
                .SingleOrDefaultAsync(cancellationToken);
        }
    }
}
