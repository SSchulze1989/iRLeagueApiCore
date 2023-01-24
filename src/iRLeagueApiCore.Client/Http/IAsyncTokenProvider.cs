namespace iRLeagueApiCore.Client.Http;

public interface IAsyncTokenProvider
{
    event EventHandler TokenChanged;
    Task<string> GetIdTokenAsync();
    Task<string> GetAccessTokenAsync();
}
