using System;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Http
{
    public interface IAsyncTokenProvider
    {
        event EventHandler TokenChanged;
        Task<string> GetTokenAsync();
    }
}