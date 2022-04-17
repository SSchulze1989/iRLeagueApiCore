﻿using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    public class GetResultsFromSeasonHandlerTests : HandlersTestsBase<GetResultsFromSeasonHandler, GetResultsFromSeasonRequest, IEnumerable<GetResultModel>>
    {
        public GetResultsFromSeasonHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetResultsFromSeasonHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultsFromSeasonRequest> validator)
        {
            return new GetResultsFromSeasonHandler(logger, dbContext, new IValidator<GetResultsFromSeasonRequest>[] { validator });
        }

        protected override GetResultsFromSeasonRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private GetResultsFromSeasonRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            return new GetResultsFromSeasonRequest(leagueId, seasonId);
        }

        protected override void DefaultAssertions(GetResultsFromSeasonRequest request, IEnumerable<GetResultModel> result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            var actualResult = dbContext.ScoredResults
                .Include(x => x.Scoring)
                .Include(x => x.ScoredResultRows)
                    .ThenInclude(x => x.ResultRow)
                        .ThenInclude(x => x.Member)
                .Include(x => x.ScoredResultRows)
                    .ThenInclude(x => x.Team)
                .Where(x => x.LeagueId == request.LeagueId).Where(x => x.Result.Session.Schedule.SeasonId == request.SeasonId).First();
            var testResult = result.First();
            AssertResultData(actualResult, testResult);            
        }

        [Fact]
        public async override Task<IEnumerable<GetResultModel>> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public async override Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, testSeasonId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testSeasonId)]
        [InlineData(testLeagueId, 42)]
        public async Task HandleNotFoundAsync(long leagueId, long seasonId)
        {
            var request = DefaultRequest(leagueId, seasonId);
            await HandleNotFoundRequestAsync(request);
        }

        private void AssertResultData(ScoredResultEntity expected, GetResultModel test)
        {
            Assert.Equal(expected.ResultId, test.SessionId);
            Assert.Equal(expected.ScoringId, test.ScoringId);
            Assert.Equal(expected.Scoring.Name, test.ScoringName);
            Assert.Equal(expected.LeagueId, test.LeagueId);
            AssertResultRowData(expected.ScoredResultRows.First(), test.ResultRows.First());
        }

        private void AssertResultRowData(ScoredResultRowEntity expected, GetResultRowModel test)
        {
            Assert.Equal(expected.BonusPoints, test.BonusPoints);
            Assert.Equal(expected.FinalPosition, test.FinalPosition);
            Assert.Equal(expected.FinalPositionChange, test.FinalPositionChange);
            Assert.Equal(expected.PenaltyPoints, test.PenaltyPoints);
            Assert.Equal(expected.RacePoints, test.RacePoints);
            Assert.Equal(expected.ResultRow.AvgLapTime, test.AvgLapTime.Ticks);
            Assert.Equal(expected.ResultRow.Car, test.Car);
            Assert.Equal(expected.ResultRow.CarClass, test.CarClass);
            Assert.Equal(expected.ResultRow.CarId, test.CarId);
            Assert.Equal(expected.ResultRow.CarNumber, test.CarNumber);
            Assert.Equal(expected.ResultRow.ClassId, test.ClassId);
            Assert.Equal(expected.ResultRow.CompletedLaps, test.CompletedLaps);
            Assert.Equal(expected.ResultRow.CompletedPct, test.CompletedPct);
            Assert.Equal(expected.ResultRow.Division, test.Division);
            Assert.Equal(expected.ResultRow.FastestLapTime, test.FastestLapTime.Ticks);
            Assert.Equal(expected.ResultRow.FastLapNr, test.FastLapNr);
            Assert.Equal(expected.ResultRow.FinishPosition, test.FinishPosition);
            Assert.Equal(expected.ResultRow.Incidents, test.Incidents);
            Assert.Equal(expected.ResultRow.Interval, test.Interval.Ticks);
            Assert.Equal(expected.ResultRow.LeadLaps, test.LeadLaps);
            Assert.Equal(expected.ResultRow.License, test.License);
            Assert.Equal(expected.ResultRow.Member.Firstname, test.Firstname);
            Assert.Equal(expected.ResultRow.Member.Lastname, test.Lastname);
            Assert.Equal(expected.ResultRow.MemberId, test.MemberId);
            Assert.Equal(expected.ResultRow.NewIrating, test.NewIrating);
            Assert.Equal(expected.ResultRow.NewLicenseLevel, test.NewLicenseLevel);
            Assert.Equal(expected.ResultRow.NewSafetyRating, test.NewSafetyRating);
            Assert.Equal(expected.ResultRow.OldIrating, test.OldIrating);
            Assert.Equal(expected.ResultRow.OldLicenseLevel, test.OldLicenseLevel);
            Assert.Equal(expected.ResultRow.OldSafetyRating, test.OldSafetyRating);
            Assert.Equal(expected.ResultRow.PositionChange, test.PositionChange);
            Assert.Equal(expected.ResultRow.QualifyingTime, test.QualifyingTime.Ticks);
            Assert.Equal(expected.ResultRow.SeasonStartIrating, test.SeasonStartIrating);
            Assert.Equal(expected.ResultRow.StartPosition, test.StartPosition);
            Assert.Equal(expected.ResultRow.Status, test.Status);
            Assert.Equal(expected.Team?.Name, test.TeamName);
            Assert.Equal(expected.TeamId, test.TeamId);
            Assert.Equal(expected.TotalPoints, test.TotalPoints);
        }
    }
}
