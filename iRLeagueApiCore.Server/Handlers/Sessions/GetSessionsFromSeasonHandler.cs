using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Sessions
{
    public record GetSessionsFromSeasonRequest(long LeagueId, long SeasonId) : IRequest<IEnumerable<GetSessionModel>>;

    public class GetSessionsFromSeasonHandler : SessionHandlerBase<GetSessionsFromSeasonHandler, GetSessionsFromSeasonRequest>,
        IRequestHandler<GetSessionsFromSeasonRequest, IEnumerable<GetSessionModel>>
    {
        public GetSessionsFromSeasonHandler(ILogger<GetSessionsFromSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSessionsFromSeasonRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<GetSessionModel>> Handle(GetSessionsFromSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSessions = await MapToGetSessionModelsAsync(request.LeagueId, request.SeasonId, cancellationToken);
            return getSessions;
        }

        private async Task<IEnumerable<GetSessionModel>> MapToGetSessionModelsAsync(long leagueId, long seasonId, CancellationToken cancellationToken)
        {
            return await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.Schedule.SeasonId == seasonId)
                .Select(MapToGetSessionModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
