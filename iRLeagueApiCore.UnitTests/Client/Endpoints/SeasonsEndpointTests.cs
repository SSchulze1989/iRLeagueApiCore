using iRLeagueApiCore.Client.Endpoints.Leagues;
using iRLeagueApiCore.Client.Endpoints.Seasons;
using iRLeagueApiCore.Client.QueryBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Client.Endpoints
{
    public class SeasonsEndpointTests
    {
        private const string testLeagueName = "testLeague";
        private const long testSeasonId = 1;

        [Fact]
        public async Task ShouldCallCorrectRequestSeasons()
        {
            var requestUrl = EndpointsTests.BaseUrl + $"{testLeagueName}/Seasons";
            await EndpointsTests.TestRequestUrl<ILeaguesEndpoint>(requestUrl,
                x => new LeaguesEndpoint(x, new RouteBuilder()),
                x => x.WithName(testLeagueName).Seasons().Get());
        }

        [Fact]
        public async Task ShouldCallCorrectRequestWithId()
        {
            var requestUrl = EndpointsTests.BaseUrl + $"{testLeagueName}/Seasons/{testSeasonId}";
            await EndpointsTests.TestRequestUrl<ILeaguesEndpoint>(requestUrl,
                x => new LeaguesEndpoint(x, new RouteBuilder()),
                x => x.WithName(testLeagueName).Seasons().WithId(testSeasonId).Get());
        }
    }
}
