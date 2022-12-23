using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models.Tracks;

namespace iRLeagueApiCore.Client.Endpoints.Tracks;

public sealed class TracksEndpoint : GetAllEndpoint<TrackGroupModel>, ITracksEndpoint
{
    public TracksEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Tracks");
    }
}
