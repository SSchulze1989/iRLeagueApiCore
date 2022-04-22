using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Sessions;
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
        public ISchedulesEndpoint Schedules();
        public ISessionsEndpoint Sessions();
        public IScoringsEndpoint Scorings();
    }
}
