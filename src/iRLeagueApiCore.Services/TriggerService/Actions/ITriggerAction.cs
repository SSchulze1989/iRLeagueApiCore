namespace iRLeagueApiCore.Services.TriggerService.Actions;
internal interface ITriggerAction
{
    public Task ExecuteAsync(Dictionary<string, object> parameters, CancellationToken cancellationToken = default);
}
