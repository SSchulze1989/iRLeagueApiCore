﻿using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    internal class ResultConfigsEndpoint : PostGetAllEndpoint<ResultConfigModel, PostResultConfigModel>, IResultConfigsEndpoint
    {
        public ResultConfigsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("ResultConfigs");
        }

        public IResultConfigByIdEndpoint WithId(long id)
        {
            return new ResultConfigByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
