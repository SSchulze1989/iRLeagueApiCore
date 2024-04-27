using iRLeagueApiCore.Common.Models.Tracks;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Tracks;

public abstract class TracksHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
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
