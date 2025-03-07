﻿using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints;

internal class CalculateEndpoint : PostEndpoint<bool>
{
    public CalculateEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) :
        base(httpClient, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Calculate");
    }
}
