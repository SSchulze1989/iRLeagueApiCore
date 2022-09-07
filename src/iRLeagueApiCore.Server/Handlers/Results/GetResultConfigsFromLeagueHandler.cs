using FluentValidation;
using iRLeagueApiCore.Common.Models;
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
    public record GetResultConfigsFromLeagueRequest(long LeagueId) : IRequest<IEnumerable<ResultConfigModel>>;

    public class GetResultConfigsFromLeagueHandler : ResultConfigHandlerBase<GetResultConfigsFromLeagueHandler, GetResultConfigsFromLeagueRequest>,
        IRequestHandler<GetResultConfigsFromLeagueRequest, IEnumerable<ResultConfigModel>>
    {
        public GetResultConfigsFromLeagueHandler(ILogger<GetResultConfigsFromLeagueHandler> logger, LeagueDbContext dbContext, 
            IEnumerable<IValidator<GetResultConfigsFromLeagueRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<ResultConfigModel>> Handle(GetResultConfigsFromLeagueRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getResults = await MapToGetResultConfigsFromLeagueAsync(request.LeagueId, cancellationToken);
            return getResults;
        }

        protected virtual async Task<IEnumerable<ResultConfigModel>> MapToGetResultConfigsFromLeagueAsync(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.ResultConfigurations
                .Where(x => x.LeagueId == leagueId)
                .Select(MapToResultConfigModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
