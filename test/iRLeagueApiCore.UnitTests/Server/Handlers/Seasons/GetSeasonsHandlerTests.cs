﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Seasons
{
    [Collection("HandlerTests")]
    public class GetSeasonsHandlerTests : HandlersTestsBase<GetSeasonsHandler, GetSeasonsRequest, IEnumerable<SeasonModel>>
    {
        public GetSeasonsHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetSeasonsHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSeasonsRequest> validator)
        {
            return new GetSeasonsHandler(logger, dbContext, new IValidator<GetSeasonsRequest>[] { validator });
        }

        protected override GetSeasonsRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private GetSeasonsRequest DefaultRequest(long leagueId)
        {
            return new GetSeasonsRequest(leagueId);
        }

        [Fact]
        public async override Task<IEnumerable<SeasonModel>> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public async Task HandleNotFound(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}