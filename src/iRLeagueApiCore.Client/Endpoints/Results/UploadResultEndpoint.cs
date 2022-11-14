using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    internal sealed class UploadResultEndpoint : PostEndpoint<string>, IPostEndpoint<string>
    {
        public UploadResultEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Upload");
        }
    }
}