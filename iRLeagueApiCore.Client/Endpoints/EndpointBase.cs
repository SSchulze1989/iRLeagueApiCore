using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Client.QueryBuilder;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public class EndpointBase
    {
        protected HttpClient HttpClient { get; }
        protected RouteBuilder RouteBuilder { get; }

        protected string QueryUrl => RouteBuilder.Build();

        public EndpointBase(HttpClient httpClient, RouteBuilder routeBuilder)
        {
            HttpClient = httpClient;
            RouteBuilder = routeBuilder;
        }
    }
}