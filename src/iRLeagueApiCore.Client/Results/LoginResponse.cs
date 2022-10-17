using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Results
{
    public struct LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expires { get; set; }
    }
}
