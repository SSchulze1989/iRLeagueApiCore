using iRLeagueApiCore.Client.Endpoints.Schedules;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;
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
    internal class SeasonsEndpoint : PostGetAllEndpoint<SeasonModel, PostSeasonModel>, ISeasonsEndpoint
    {
        public SeasonsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Seasons");
        }

        IGetEndpoint<SeasonModel> ISeasonsEndpoint.Current()
        {
            return new CurrentSeasonEndpoint(HttpClientWrapper, RouteBuilder);
        }

        ISeasonByIdEndpoint IWithIdEndpoint<ISeasonByIdEndpoint>.WithId(long id)
        { 
            return new SeasonByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
