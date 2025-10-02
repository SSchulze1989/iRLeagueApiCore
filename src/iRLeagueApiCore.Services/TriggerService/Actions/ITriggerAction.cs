using iRLeagueApiCore.Common.Enums;

namespace iRLeagueApiCore.Services.TriggerService.Actions;
public interface ITriggerAction
{
    public Task ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
}
