using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public record GetSeasonsRequest(long LeagueId) : IRequest<IEnumerable<GetSeasonModel>>;

    public class GetSeasonsHandler : SeasonHandlerBase<GetSeasonsHandler, GetSeasonsRequest>, IRequestHandler<GetSeasonsRequest, IEnumerable<GetSeasonModel>>
    {
        public GetSeasonsHandler(ILogger<GetSeasonsHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSeasonsRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public Task<IEnumerable<GetSeasonModel>> Handle(GetSeasonsRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
