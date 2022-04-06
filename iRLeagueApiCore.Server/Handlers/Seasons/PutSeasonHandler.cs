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

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public record PutSeasonRequest(long LeagueId, LeagueUser User, long SeasonId, PutSeasonModel Model) : IRequest<GetSeasonModel>;

    public class PutSeasonHandler : SeasonHandlerBase<PutSeasonHandler, PutSeasonRequest>, IRequestHandler<PutSeasonRequest, GetSeasonModel>
    {
        public PutSeasonHandler(ILogger<PutSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutSeasonRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<GetSeasonModel> Handle(PutSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putSeason = await GetSeasonEntityAsync(request.LeagueId, request.SeasonId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            await MapToSeasonEntityAsync(request.LeagueId, request.User, request.Model, putSeason, cancellationToken);
            await dbContext.SaveChangesAsync();
            var getSeason = await MapToGetSeasonModel(request.LeagueId, request.SeasonId, cancellationToken);
            return getSeason;
        }
    }
}
