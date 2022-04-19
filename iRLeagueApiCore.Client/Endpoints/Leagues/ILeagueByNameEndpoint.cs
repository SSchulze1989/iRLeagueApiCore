using iRLeagueApiCore.Client.Endpoints.Seasons;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    public interface ILeagueByNameEndpoint
    {
        public ISeasonsEndpoint Seasons();
    }
}
