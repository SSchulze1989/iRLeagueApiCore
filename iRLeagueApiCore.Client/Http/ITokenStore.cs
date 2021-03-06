using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Http
{
    public interface ITokenStore : IAsyncTokenProvider
    {
        bool IsLoggedIn { get; }
        Task SetTokenAsync(string token);
        Task ClearTokenAsync();
    }
}
