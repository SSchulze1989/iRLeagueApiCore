
using iRLeagueApiCore.Common.Models.Rosters;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public abstract class RostersHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public RostersHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    protected async Task<RosterEntity?> GetRosterEntity(long rosterId, CancellationToken cancellationToken)
    {
        return await dbContext.Rosters
            .Where(x => x.RosterId == rosterId)
            .Include(x => x.RosterEntries)
                .ThenInclude(x => x.Member)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected RosterEntity MapToRosterEntity(RosterEntity entity, PostRosterModel model, LeagueUser user)
    {
        entity.Name = model.Name;
        entity.Description = model.Description;
        return UpdateVersionEntity(user, entity);
    }

    protected ICollection<RosterEntryEntity> MapRosterEntries(ICollection<RosterEntryEntity> entities, IEnumerable<RosterEntryModel> models)
    {
        foreach (var entryModel in models)
        {
            var entryEntity = entities.FirstOrDefault(x => x.MemberId == entryModel.MemberId);
            if (entryEntity is null)
            {
                entryEntity = new RosterEntryEntity()
                {
                    MemberId = entryModel.MemberId,
                };
                entities.Add(entryEntity);
            }
            entryEntity.TeamId = entryModel.TeamId;
        }

        return entities;
    }

    protected Expression<Func<RosterEntity, RosterInfoModel>> MapToRosterInfoModelExpression => roster => new()
    {
        RosterId = roster.RosterId,
        Name = roster.Name,
        Description = roster.Description,
        EntryCount = roster.RosterEntries.Count,
    };

    protected async Task<RosterModel?> GetRosterModel(long rosterId, CancellationToken cancellationToken)
    {
        return await dbContext.Rosters
            .Where(x => x.RosterId == rosterId)
            .Select(MapToRosterModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected Expression<Func<RosterEntity, RosterModel>> MapToRosterModelExpression => roster => new()
    {
        RosterId = roster.RosterId,
        Name = roster.Name,
        Description = roster.Description,
        RosterEntries = roster.RosterEntries.Select(entry => new RosterEntryInfoModel()
        {
            MemberId = entry.MemberId,
            Firstname = entry.Member.Member.Firstname,
            Lastname = entry.Member.Member.Lastname,
            TeamId = entry.TeamId,
            TeamName = entry.TeamId != null ? entry.Team.Name : "",
            TeamColor = entry.TeamId != null ? entry.Team.TeamColor : "",
        }).ToList(),
    };
}
