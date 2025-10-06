namespace iRLeagueApiCore.Server.Webhooks;
public interface ILeagueWebhook
{
    public Task SendAsync(object? data, string url, CancellationToken cancellationToken = default);
}
