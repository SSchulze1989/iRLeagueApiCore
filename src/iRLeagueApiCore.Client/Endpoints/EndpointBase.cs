using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Client.QueryBuilder;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using iRLeagueApiCore.Client.Http;

namespace iRLeagueApiCore.Client.Endpoints
{
    public class EndpointBase
    {
        protected HttpClientWrapper HttpClientWrapper { get; }
        protected RouteBuilder RouteBuilder { get; }

        protected string QueryUrl => RouteBuilder.Build();

        public EndpointBase(HttpClientWrapper httpClient, RouteBuilder routeBuilder)
        {
            HttpClientWrapper = httpClient;
            RouteBuilder = routeBuilder.Copy();
        }
    }
}