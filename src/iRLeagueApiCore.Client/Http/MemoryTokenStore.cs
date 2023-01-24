namespace iRLeagueApiCore.Client.Http;
internal sealed class MemoryTokenStore : ITokenStore
{
    private string token = string.Empty;

    public bool IsLoggedIn => string.IsNullOrEmpty(token) == false;

    public event EventHandler? TokenChanged;

    public Task ClearTokenAsync()
    {
        token = string.Empty;
        TokenChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }

    public Task<string> GetTokenAsync()
    {
        return Task.FromResult(token);
    }

    public Task SetTokenAsync(string token)
    {
        this.token = token;
        TokenChanged?.Invoke(this, EventArgs.Empty);
        return Task.CompletedTask;
    }
}
