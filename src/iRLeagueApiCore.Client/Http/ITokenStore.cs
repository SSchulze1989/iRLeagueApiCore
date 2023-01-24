namespace iRLeagueApiCore.Client.Http;

public interface ITokenStore : IAsyncTokenProvider
{
    bool IsLoggedIn { get; }
    Task SetIdTokenAsync(string token);
    Task SetAccessTokenAsync(string token);
    Task ClearTokensAsync();
}
