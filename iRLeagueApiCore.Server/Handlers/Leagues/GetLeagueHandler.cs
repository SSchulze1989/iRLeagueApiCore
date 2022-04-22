using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public record GetLeagueRequest(long leagueId) : IRequest<GetLeagueModel>;

    public class GetLeagueHandler : LeagueHandlerBase<GetLeagueHandler, GetLeagueRequest>, IRequestHandler<GetLeagueRequest, GetLeagueModel>
    {
        public GetLeagueHandler(ILogger<GetLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetLeagueRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<GetLeagueModel> Handle(GetLeagueRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getLeague = await MapToGetLeagueModelAsync(request.leagueId, cancellationToken) ?? throw new ResourceNotFoundException();
            return getLeague;
        }
    }
}
