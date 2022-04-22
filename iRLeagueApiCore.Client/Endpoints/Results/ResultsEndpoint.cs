using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    internal class ResultsEndpoint : GetAllEndpoint<GetResultModel>, IResultsEndpoint
    {
        public ResultsEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) : 
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Results");
        }
    }
}
