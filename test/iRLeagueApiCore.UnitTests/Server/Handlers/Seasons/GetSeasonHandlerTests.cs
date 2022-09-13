﻿using FluentAssertions;
using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Seasons
{
    [Collection("HandlerTests")]
    public class GetSeasonHandlerTests : HandlersTestsBase<GetSeasonHandler, GetSeasonRequest, SeasonModel>
    {
        public GetSeasonHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetSeasonRequest> validator)
        {
            return new GetSeasonHandler(logger, dbContext, new IValidator<GetSeasonRequest>[] { validator });
        }

        protected override GetSeasonRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        private GetSeasonRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            return new GetSeasonRequest(leagueId, seasonId);
        }

        protected override void DefaultAssertions(GetSeasonRequest request, SeasonModel result, LeagueDbContext dbContext)
        {
            var testSeason = dbContext.Seasons
                .Include(x => x.Schedules)
                .SingleOrDefault(x => x.SeasonId == request.SeasonId);
            result.LeagueId.Should().Be(request.LeagueId);
            result.SeasonId.Should().Be(request.SeasonId);
            result.Finished.Should().Be(testSeason.Finished);
            result.HideComments.Should().Be(testSeason.HideCommentsBeforeVoted);
            result.MainScoringId.Should().Be(testSeason.MainScoringScoringId);
            result.ScheduleIds.Should().BeEquivalentTo(testSeason.Schedules.Select(x => x.ScheduleId));
            result.SeasonEnd.Should().Be(testSeason.SeasonEnd);
            result.SeasonStart.Should().Be(testSeason.SeasonStart);
            result.SeasonName.Should().Be(testSeason.SeasonName);
            AssertVersion(testSeason, result);
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public async override Task<SeasonModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Fact]
        public override async Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(1, 42)]
        [InlineData(42, 1)]
        public async Task HandleNotFound(long leagueId, long seasonId)
        {
            var request = DefaultRequest(leagueId, seasonId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}