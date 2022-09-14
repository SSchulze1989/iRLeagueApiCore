﻿using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Reviews;
using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Endpoints.Scorings;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Common.Models;
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
        IEventsEndpoint Events();
        IResultConfigsEndpoint ResultConfigs();
        IPointRulesEndpoint PointRules();
        IReviewsEndpoint Reviews();
        IReviewCommentsEndpoint ReviewComments();
    }
}
