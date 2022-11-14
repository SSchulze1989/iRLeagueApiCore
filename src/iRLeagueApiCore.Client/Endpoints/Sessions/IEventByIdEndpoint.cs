﻿using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Common.Models.Standings;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface IEventByIdEndpoint : IUpdateEndpoint<EventModel, PutEventModel>
    {
        IEventResultsEndpoint Results();
        IGetAllEndpoint<ReviewModel> Reviews();
        IGetAllEndpoint<MemberInfoModel> Members();
        IGetAllEndpoint<StandingsModel> Standings();
    }
}