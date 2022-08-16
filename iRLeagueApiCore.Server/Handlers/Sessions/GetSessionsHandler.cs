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

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public record GetSessionsRequest(long LeagueId) : IRequest<IEnumerable<SessionModel>>;

    public class GetSessionsHandler : SessionHandlerBase<GetSessionsHandler, GetSessionsRequest>, IRequestHandler<GetSessionsRequest, IEnumerable<SessionModel>>
    {
        public GetSessionsHandler(ILogger<GetSessionsHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSessionsRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<SessionModel>> Handle(GetSessionsRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSessions = await MapToGetSessionModelsAsync(request.LeagueId, cancellationToken);
            if (getSessions.Count() == 0)
            {
                throw new ResourceNotFoundException();
            }
            return getSessions;
        }

        protected virtual async Task<IEnumerable<SessionModel>> MapToGetSessionModelsAsync(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Select(MapToGetSessionModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
