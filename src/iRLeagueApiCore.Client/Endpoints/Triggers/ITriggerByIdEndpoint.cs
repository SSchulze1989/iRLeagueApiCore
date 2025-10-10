using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Triggers;

public interface ITriggerByIdEndpoint : IUpdateEndpoint<TriggerModel, PutTriggerModel>
{
    public IPostEndpoint<NoContent> RunTrigger();
}