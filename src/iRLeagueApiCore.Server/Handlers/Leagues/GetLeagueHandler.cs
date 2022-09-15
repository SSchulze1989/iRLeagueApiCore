using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public record GetLeagueRequest(long leagueId) : IRequest<LeagueModel>;

    public class GetLeagueHandler : LeagueHandlerBase<GetLeagueHandler, GetLeagueRequest>, IRequestHandler<GetLeagueRequest, LeagueModel>
    {
        public GetLeagueHandler(ILogger<GetLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetLeagueRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<LeagueModel> Handle(GetLeagueRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getLeague = await MapToGetLeagueModelAsync(request.leagueId, cancellationToken) ?? throw new ResourceNotFoundException();
            return getLeague;
        }
    }
}
