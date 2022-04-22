using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Schedules
{
    internal class ScheduleByIdEndpoint : UpdateEndpoint<GetScheduleModel, PutScheduleModel>, IScheduleByIdEndpoint
    {
        public ScheduleByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long scheduleId) : 
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddParameter(scheduleId);
        }

        public IPostEndpoint<GetSessionModel, PostSessionModel> Sessions()
        {
            return new SessionsEndpoint(HttpClient, RouteBuilder);
        }
    }
}