using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public record PostSeasonRequest(long LeagueId, LeagueUser user, PostSeasonModel Model) : IRequest<GetSeasonModel>;

    public class PostSeasonHandler : SeasonHandlerBase<PostSeasonHandler, PostSeasonRequest>, IRequestHandler<PostSeasonRequest, GetSeasonModel>
    {
        public PostSeasonHandler(ILogger<PostSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostSeasonRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public Task<GetSeasonModel> Handle(PostSeasonRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
