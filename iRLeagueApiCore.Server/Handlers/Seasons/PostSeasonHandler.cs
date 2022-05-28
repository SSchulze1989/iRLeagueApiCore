using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Seasons
{
    public record PostSeasonRequest(long LeagueId, LeagueUser user, PostSeasonModel Model) : IRequest<SeasonModel>;

    public class PostSeasonHandler : SeasonHandlerBase<PostSeasonHandler, PostSeasonRequest>, IRequestHandler<PostSeasonRequest, SeasonModel>
    {
        public PostSeasonHandler(ILogger<PostSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostSeasonRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<SeasonModel> Handle(PostSeasonRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            
            var postSeason = await CreateSeasonEntity(request.LeagueId, cancellationToken);
            await MapToSeasonEntityAsync(request.LeagueId, request.user, request.Model, postSeason, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getSeason = await MapToGetSeasonModel(request.LeagueId, postSeason.SeasonId, cancellationToken)
                ?? throw new InvalidOperationException($"Creating season {request.Model.SeasonName} failed");
            return getSeason;
        }

        protected async Task<SeasonEntity> CreateSeasonEntity(long leagueId, CancellationToken cancellationToken = default)
        {
            var league = await dbContext.Leagues
                .SingleOrDefaultAsync(x => x.Id == leagueId) ?? throw new ResourceNotFoundException();
            var seasonEntity = new SeasonEntity();
            league.Seasons.Add(seasonEntity);
            return seasonEntity;
        }
    }
}
