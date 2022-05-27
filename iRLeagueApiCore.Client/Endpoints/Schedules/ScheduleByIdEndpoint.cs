using iRLeagueApiCore.Client.Endpoints.Sessions;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Schedules
{
    internal class ScheduleByIdEndpoint : UpdateEndpoint<GetScheduleModel, PutScheduleModel>, IScheduleByIdEndpoint
    {
        public ScheduleByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long scheduleId) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddParameter(scheduleId);
        }

        public IPostGetAllEndpoint<SessionModel, PostSessionModel> Sessions()
        {
            return new SessionsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}