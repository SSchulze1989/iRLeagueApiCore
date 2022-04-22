using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    internal class SeasonsEndpoint : PostGetAllEndpoint<GetSeasonModel, PostSeasonModel>, ISeasonsEndpoint
    {
        public SeasonsEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) :
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Seasons");
        }

        ISeasonByIdEndpoint IWithIdEndpoint<ISeasonByIdEndpoint>.WithId(long id)
        { 
            return new SeasonByIdEndpoint(HttpClient, RouteBuilder, id);
        }
    }
}
