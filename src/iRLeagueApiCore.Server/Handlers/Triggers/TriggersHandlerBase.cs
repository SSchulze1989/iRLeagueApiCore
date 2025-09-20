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
        entity.Parameters = model.Parameters != null ? new Dictionary<string, object>(model.Parameters) : new Dictionary<string, object>();
        entity.Action = model.Action;
        entity.ActionParameters = model.ActionParameters != null ? new Dictionary<string, object>(model.ActionParameters) : new Dictionary<string, object>();
        return entity;
    }

    protected Expression<Func<TriggerEntity, TriggerModel>> MapToTriggerExpression => trigger => new TriggerModel()
    {
        TriggerId = trigger.TriggerId,
        Name = trigger.Name,
        Description = trigger.Description,
        TriggerType = trigger.TriggerType,
        Parameters = trigger.Parameters,
        Action = trigger.Action,
        ActionParameters = trigger.ActionParameters,
    };
}
