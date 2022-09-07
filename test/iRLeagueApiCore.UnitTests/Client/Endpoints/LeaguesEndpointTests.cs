using iRLeagueApiCore.Client.Endpoints;
using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using Microsoft.AspNetCore.Identity.Test;
using Moq;
using Moq.Protected;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Client.Endpoints
{
    public class LeaguesEndpointTests
    {
        private const string testLeagueName = "testLeague";
        private const long testLeagueId = 1;

        [Fact]
        public async Task ShouldCallCorrectRequestUrlLeagues()
        {
            var shouldRequestUrl = EndpointsTests.BaseUrl + "Leagues";
            await EndpointsTests.TestRequestUrl<ILeaguesEndpoint>(shouldRequestUrl, x => new LeaguesEndpoint(x, new RouteBuilder()), x => x.Get()); 
        }

        [Fact]
        public async Task ShouldCallCorrectRequestUrlWithId()
        {
            var requestUrl = EndpointsTests.BaseUrl + $"Leagues/{testLeagueId}";
            await EndpointsTests.TestRequestUrl<ILeaguesEndpoint>(requestUrl,
                x => new LeaguesEndpoint(x, new RouteBuilder()),
                x => x.WithId(1).Get());
        }

        [Fact]
        public async Task ShouldCallCorrectRequestUrlWithName()
        {
            var requestUrl = EndpointsTests.BaseUrl + $"{testLeagueName}/Seasons";
            await EndpointsTests.TestRequestUrl<ILeaguesEndpoint>(requestUrl,
                x => new LeaguesEndpoint(x, new RouteBuilder()),
                x => x.WithName(testLeagueName).Seasons().Get());
        }
    }
}
