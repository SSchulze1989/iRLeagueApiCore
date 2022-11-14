﻿using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    internal class ResultsEndpoint : GetAllEndpoint<EventResultModel>, 
        IResultsEndpoint, IWithIdEndpoint<IResultByIdEndpoint>, IEventResultsEndpoint
    {
        public ResultsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Results");
        }

        public IPostEndpoint<string> Upload()
        {
            return new UploadResultEndpoint(HttpClientWrapper, RouteBuilder);
        }

        public IResultByIdEndpoint WithId(long id)
        {
            return new ResultByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
