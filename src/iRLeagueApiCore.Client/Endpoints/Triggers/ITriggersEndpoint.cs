using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Triggers;
public interface ITriggersEndpoint : IWithIdEndpoint<ITriggerByIdEndpoint>, IPostGetAllEndpoint<TriggerModel, PostTriggerModel>
{
}
