using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.ResultsParsing;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    internal sealed class UploadResultEndpoint : PostEndpoint<ParseSimSessionResult>, IPostEndpoint<ParseSimSessionResult>
    {
        public UploadResultEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Upload");
        }
    }
}