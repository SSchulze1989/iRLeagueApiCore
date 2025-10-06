using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.TriggerService.Actions;
public interface ITriggerAction
{
    public Task ExecuteAsync(TriggerParameterModel triggerParameter, Dictionary<string, object> actionParameters, CancellationToken cancellationToken = default);
}
