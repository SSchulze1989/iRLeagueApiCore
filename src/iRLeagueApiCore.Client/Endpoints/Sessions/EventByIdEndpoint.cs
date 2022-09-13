using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    internal class EventByIdEndpoint : UpdateEndpoint<EventModel, PutEventModel>, IEventByIdEndpoint
    {
        public EventByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long EventId) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddParameter(EventId);
        }

        IGetAllEndpoint<EventResultModel> IEventByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}