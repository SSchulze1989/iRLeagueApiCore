namespace iRLeagueApiCore.Client.Http
{
    public interface ITokenStore : IAsyncTokenProvider
    {
        bool IsLoggedIn { get; }
        Task SetTokenAsync(string token);
        Task ClearTokenAsync();
    }
}
