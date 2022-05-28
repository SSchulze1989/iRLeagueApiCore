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

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public record GetLeaguesRequest() : IRequest<IEnumerable<LeagueModel>>;

    public class GetLeaguesHandler : LeagueHandlerBase<GetLeaguesHandler, GetLeaguesRequest>, IRequestHandler<GetLeaguesRequest, IEnumerable<LeagueModel>>
    {
        public GetLeaguesHandler(ILogger<GetLeaguesHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetLeaguesRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<LeagueModel>> Handle(GetLeaguesRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getLeague = await MapToGetLeagueModelsAsync(cancellationToken);
            return getLeague;
        }

        public async Task<IEnumerable<LeagueModel>> MapToGetLeagueModelsAsync(CancellationToken cancellationToken)
        {
            return await dbContext.Leagues
                .Select(MapToGetLeagueModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
