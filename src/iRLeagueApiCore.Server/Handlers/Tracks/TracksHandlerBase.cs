using FluentValidation;
using iRLeagueApiCore.Common.Models.Tracks;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Tracks
{
    public class TracksHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public TracksHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        protected async Task<IEnumerable<TrackGroupModel>> MapToTrackGroupModels(CancellationToken cancellationToken)
        {
            return await dbContext.TrackGroups
                .Select(MapToTrackGroupExpression)
                .ToListAsync(cancellationToken);
        }

        protected static Expression<Func<TrackGroupEntity, TrackGroupModel>> MapToTrackGroupExpression => group => new TrackGroupModel()
        {
            TrackGroupId = group.TrackGroupId,
            TrackName = group.TrackName,
            Configs = group.TrackConfigs.Select(config => new TrackConfigModel()
            {
                TrackId = config.TrackId,
                ConfigName = config.ConfigName,
                Turns = config.Turns,
                TrackName = group.TrackName,
                Type = config.ConfigType,
                HasNightLighting = config.HasNightLighting,
                Length = config.LengthKm,
            })
        };
    }
}
