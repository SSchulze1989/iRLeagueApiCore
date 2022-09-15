using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    internal class ResultsEndpoint : GetAllEndpoint<EventResultModel>, IResultsEndpoint,
        IGetAllEndpoint<EventResultModel>, IWithIdEndpoint<IResultByIdEndpoint>
    {
        public ResultsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Results");
        }

        public IResultByIdEndpoint WithId(long id)
        {
            return new ResultByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
