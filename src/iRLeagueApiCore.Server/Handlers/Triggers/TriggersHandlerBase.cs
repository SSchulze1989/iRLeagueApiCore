using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Triggers;

public abstract class TriggersHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public TriggersHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    protected TriggerEntity MapToTriggerEntity(TriggerEntity entity, PostTriggerModel model)
    {
        entity.Name = model.Name;
        entity.Description = model.Description;
        entity.TriggerType = model.TriggerType;
        entity.Interval = model.Interval;
        entity.EventType = model.EventType;
        entity.IsArchived = !model.IsActive;
        entity.RefId1 = model.RefId1;
        entity.RefId2 = model.RefId2;
        entity.ActionParameters = model.ActionParameters;
        entity.TimeElapses = entity.TriggerType switch
        {
            TriggerType.Time => model.Time?.ToUniversalTime(),
            TriggerType.Interval => DateTimeOffset.UtcNow.Add(model.Interval ?? TimeSpan.Zero),
            _ => null
        };
        return entity;
    }

    protected Expression<Func<TriggerEntity, TriggerModel>> MapToTriggerExpression => trigger => new TriggerModel()
    {
        TriggerId = trigger.TriggerId,
        Name = trigger.Name,
        Description = trigger.Description,
        TriggerType = trigger.TriggerType,
        EventType = trigger.EventType,
        Interval = trigger.Interval,
        Time = trigger.TimeElapses,
        RefId1 = trigger.RefId1,
        RefId2 = trigger.RefId2,
        Action = trigger.Action,
        ActionParameters = trigger.ActionParameters,
        IsActive = !trigger.IsArchived,
    };
}
