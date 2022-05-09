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
        string Name { get; }
        ISeasonsEndpoint Seasons();
        ISchedulesEndpoint Schedules();
        ISessionsEndpoint Sessions();
        IScoringsEndpoint Scorings();
    }
}
