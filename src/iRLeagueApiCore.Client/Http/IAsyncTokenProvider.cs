using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Http
{
    public interface IAsyncTokenProvider
    {
        Task<string> GetTokenAsync();
    }
}