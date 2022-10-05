﻿using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models.Reviews;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Reviews
{
    public class MoveToSessionEndpoint : PostEndpoint<ReviewModel>, IPostEndpoint<ReviewModel>
    {
        public MoveToSessionEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder, long sessionId) : 
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddEndpoint("MoveToSession");
            RouteBuilder.AddParameter(sessionId);
        }
    }
}