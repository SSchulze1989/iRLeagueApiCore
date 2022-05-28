using FluentValidation;
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
    [Collection("HandlerTests")]
    public class GetResultHandlerTests : HandlersTestsBase<GetResultHandler, GetResultRequest, ResultModel>
    {
        public GetResultHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override GetResultHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetResultRequest> validator)
        {
            return new GetResultHandler(logger, dbContext, new IValidator<GetResultRequest>[] { validator });
        }

        protected override GetResultRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        private GetResultRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId, long scoringId = testScoringId)
        {
            return new GetResultRequest(leagueId, seasonId, scoringId);
        }

        protected override void DefaultAssertions(GetResultRequest request, ResultModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            var actualResult = dbContext.ScoredResults
                .Where(x => x.LeagueId == request.LeagueId)
                .Where(x => x.ScoringId == request.ScoringId)
                .Where(x => x.Result.Session.SessionId == request.SessionId)
                .Include(x => x.Scoring)
                .Include(x => x.ScoredResultRows)
                        .ThenInclude(x => x.Member)
                .Include(x => x.ScoredResultRows)
                    .ThenInclude(x => x.Team)
                .Single();
            AssertResultData(actualResult, result);
        }

        [Fact]
        public async override Task<ResultModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public async override Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, testSeasonId, testScoringId)]
        [InlineData(testLeagueId, 0, testScoringId)]
        [InlineData(testLeagueId, testSeasonId, 0)]
        [InlineData(42, testSeasonId, testScoringId)]
        [InlineData(testLeagueId, 42, testScoringId)]
        [InlineData(testLeagueId, testSeasonId, 42)]
        public async Task HandleNotFoundAsync(long leagueId, long seasonId, long scoringId)
        {
            var request = DefaultRequest(leagueId, seasonId, scoringId);
            await HandleNotFoundRequestAsync(request);
        }

        private void AssertResultData(ScoredResultEntity expected, ResultModel test)
        {
            Assert.Equal(expected.ResultId, test.SessionId);
            Assert.Equal(expected.ScoringId, test.ScoringId);
            Assert.Equal(expected.Scoring.Name, test.ScoringName);
            Assert.Equal(expected.LeagueId, test.LeagueId);
            AssertResultRowData(expected.ScoredResultRows.First(), test.ResultRows.First());
        }

        private void AssertResultRowData(ScoredResultRowEntity expected, ResultRowModel test)
        {
            Assert.Equal(expected.BonusPoints, test.BonusPoints);
            Assert.Equal(expected.FinalPosition, test.FinalPosition);
            Assert.Equal(expected.FinalPositionChange, test.FinalPositionChange);
            Assert.Equal(expected.PenaltyPoints, test.PenaltyPoints);
            Assert.Equal(expected.RacePoints, test.RacePoints);
            Assert.Equal(expected.AvgLapTime, test.AvgLapTime.Ticks);
            Assert.Equal(expected.Car, test.Car);
            Assert.Equal(expected.CarClass, test.CarClass);
            Assert.Equal(expected.CarId, test.CarId);
            Assert.Equal(expected.CarNumber, test.CarNumber);
            Assert.Equal(expected.ClassId, test.ClassId);
            Assert.Equal(expected.CompletedLaps, test.CompletedLaps);
            Assert.Equal(expected.CompletedPct, test.CompletedPct);
            Assert.Equal(expected.Division, test.Division);
            Assert.Equal(expected.FastestLapTime, test.FastestLapTime.Ticks);
            Assert.Equal(expected.FastLapNr, test.FastLapNr);
            Assert.Equal(expected.FinishPosition, test.FinishPosition);
            Assert.Equal(expected.Incidents, test.Incidents);
            Assert.Equal(expected.Interval, test.Interval.Ticks);
            Assert.Equal(expected.LeadLaps, test.LeadLaps);
            Assert.Equal(expected.License, test.License);
            Assert.Equal(expected.Member.Firstname, test.Firstname);
            Assert.Equal(expected.Member.Lastname, test.Lastname);
            Assert.Equal(expected.MemberId, test.MemberId);
            Assert.Equal(expected.NewIRating, test.NewIrating);
            Assert.Equal(expected.NewLicenseLevel, test.NewLicenseLevel);
            Assert.Equal(expected.NewSafetyRating, test.NewSafetyRating);
            Assert.Equal(expected.OldIRating, test.OldIrating);
            Assert.Equal(expected.OldLicenseLevel, test.OldLicenseLevel);
            Assert.Equal(expected.OldSafetyRating, test.OldSafetyRating);
            Assert.Equal(expected.PositionChange, test.PositionChange);
            Assert.Equal(expected.QualifyingTime, test.QualifyingTime.Ticks);
            Assert.Equal(expected.SeasonStartIRating, test.SeasonStartIrating);
            Assert.Equal(expected.StartPosition, test.StartPosition);
            Assert.Equal(expected.Status, test.Status);
            Assert.Equal(expected.Team?.Name, test.TeamName);
            Assert.Equal(expected.TeamId, test.TeamId);
            Assert.Equal(expected.TotalPoints, test.TotalPoints);
        }
    }
}
