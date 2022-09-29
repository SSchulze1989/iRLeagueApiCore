using iRLeagueApiCore.Client.Endpoints.Members;
using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Client.Endpoints.Reviews;
using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    internal class EventByIdEndpoint : UpdateEndpoint<EventModel, PutEventModel>, IEventByIdEndpoint
    {
        public EventByIdEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder, long EventId) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddParameter(EventId);
        }

        public IGetAllEndpoint<MemberInfoModel> Members()
        {
            return new MembersEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IGetAllEndpoint<EventResultModel> IEventByIdEndpoint.Results()
        {
            return new ResultsEndpoint(HttpClientWrapper, RouteBuilder);
        }

        IGetAllEndpoint<ReviewModel> IEventByIdEndpoint.Reviews()
        {
            return new ReviewsEndpoint(HttpClientWrapper, RouteBuilder);
        }
    }
}