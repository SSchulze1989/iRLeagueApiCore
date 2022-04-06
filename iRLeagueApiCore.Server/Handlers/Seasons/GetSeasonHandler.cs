using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
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

        public async Task<GetSeasonModel> Handle(GetSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getSeason = await MapToGetSeasonModel(request.LeagueId, request.SeasonId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getSeason;
        }
    }
}
