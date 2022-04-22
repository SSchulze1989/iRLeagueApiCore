using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Schedules
{
    internal class SchedulesEndpoint : PostGetAllEndpoint<GetScheduleModel, PostScheduleModel>, ISchedulesEndpoint
    {
        public SchedulesEndpoint(HttpClient httpClient, RouteBuilder routeBuilder) : base(httpClient, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Schedules");
        }

        IScheduleByIdEndpoint IWithIdEndpoint<IScheduleByIdEndpoint>.WithId(long id)
        {
            return new ScheduleByIdEndpoint(HttpClient, RouteBuilder, id);
        }
    }
}
