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
    public record GetSeasonRequest(long LeagueId, long SeasonId) : IRequest<GetSeasonModel>;

    public class GetSeasonHandler : SeasonHandlerBase<GetSeasonHandler, GetSeasonRequest>, IRequestHandler<GetSeasonRequest, GetSeasonModel>
    {
        public GetSeasonHandler(ILogger<GetSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetSeasonRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public Task<GetSeasonModel> Handle(GetSeasonRequest request, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
