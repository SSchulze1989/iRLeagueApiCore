using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public class LeagueHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>

    {
        public LeagueHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected virtual LeagueEntity MapToLeagueEntity(LeagueUser user, PostLeagueModel postLeague, LeagueEntity leagueEntity)
        {
            var createdOn = DateTime.UtcNow;
            leagueEntity.Name = postLeague.Name;
            leagueEntity.NameFull = postLeague.NameFull;
            leagueEntity.CreatedOn = createdOn;
            leagueEntity.LastModifiedOn = createdOn;
            leagueEntity.CreatedByUserId = user.Id;
            leagueEntity.LastModifiedByUserId = user.Id;
            leagueEntity.CreatedByUserName = user.Name;
            leagueEntity.LastModifiedByUserName = user.Name;
            return leagueEntity;
        }

        protected virtual async Task<GetLeagueModel> MapToGetLeagueModelAsync(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.Leagues
                .Where(x => x.Id == leagueId)
                .Select(x => new GetLeagueModel()
                {
                    Id = x.Id,
                    Name = x.Name,
                    NameFull = x.NameFull,
                    SeasonIds = x.Seasons
                        .Select(season => season.SeasonId)
                        .ToList(),
                    CreatedByUserId = x.CreatedByUserId,
                    CreatedByUserName = x.CreatedByUserName,
                    CreatedOn = x.CreatedOn,
                    LastModifiedByUserId = x.LastModifiedByUserId,
                    LastModifiedByUserName = x.LastModifiedByUserName,
                    LastModifiedOn = x.LastModifiedOn,
                })
                .SingleOrDefaultAsync(cancellationToken);
        }

        protected virtual async Task<LeagueEntity> GetLeagueEntityAsync(long leagueId)
        {
            return await dbContext.Leagues
                .SingleOrDefaultAsync(x => x.Id == leagueId);
        }

        protected virtual LeagueEntity MapToLeagueEntity(long leagueId, LeagueUser user, PutLeagueModel putLeague, LeagueEntity leagueEntity)
        {
            leagueEntity.NameFull = putLeague.NameFull;
            leagueEntity.LastModifiedOn = DateTime.UtcNow;
            leagueEntity.LastModifiedByUserId = user.Id;
            leagueEntity.LastModifiedByUserName = user.Name;
            return leagueEntity;
        }
    }
}
