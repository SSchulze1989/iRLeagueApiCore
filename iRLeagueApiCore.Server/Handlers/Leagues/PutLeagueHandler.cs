using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public record PutLeagueRequest(long LeagueId, LeagueUser User, PutLeagueModel Model) : IRequest<GetLeagueModel>;

    public class PutLeagueHandler : LeagueHandlerBase<PutLeagueHandler, PutLeagueRequest>, IRequestHandler<PutLeagueRequest, GetLeagueModel>
    {
        public PutLeagueHandler(ILogger<PutLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutLeagueRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<GetLeagueModel> Handle(PutLeagueRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putLeague = await GetLeagueEntityAsync(request.LeagueId) ?? throw new ResourceNotFoundException();
            MapToLeagueEntity(request.LeagueId, request.User, request.Model, putLeague);
            await dbContext.SaveChangesAsync();
            var getLeague = await MapToGetLeagueModelAsync(putLeague.Id, cancellationToken);
            return getLeague;
        }
    }
}
