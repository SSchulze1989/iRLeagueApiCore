using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
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
    public class GetResultHandlerTests : HandlersTestsBase<GetResultHandler, GetResultRequest, EventResultModel>
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

        private GetResultRequest DefaultRequest(long leagueId = testLeagueId, long resultId = testResultId)
        {
            return new GetResultRequest(leagueId, resultId);
        }

        protected override void DefaultAssertions(GetResultRequest request, EventResultModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            var actualResult = dbContext.ScoredEventResults
                .Where(x => x.LeagueId == request.LeagueId)
                .Where(x => x.ResultId == request.ResultId)
                .Include(x => x.ScoredSessionResults)
                    .ThenInclude(x => x.ScoredResultRows)
                        .ThenInclude(x => x.Member)
                .Include(x => x.ScoredSessionResults)
                    .ThenInclude(x => x.ScoredResultRows)
                        .ThenInclude(x => x.Team)
                .Single();
            AssertEventResultData(actualResult, result);
        }

        [Fact]
        public async override Task<EventResultModel> ShouldHandleDefaultAsync()
        {
            return await base.ShouldHandleDefaultAsync();
        }

        [Fact]
        public async override Task ShouldHandleValidationFailedAsync()
        {
            await base.ShouldHandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, testResultId)]
        [InlineData(testLeagueId, 0)]
        [InlineData(42, testResultId)]
        [InlineData(testLeagueId, 42)]
        public async Task HandleNotFoundAsync(long leagueId, long resultId)
        {
            var request = DefaultRequest(leagueId, resultId);
            await HandleNotFoundRequestAsync(request);
        }

        private void AssertEventResultData(ScoredEventResultEntity expected, EventResultModel test)
        {
            Assert.Equal(expected.LeagueId, test.LeagueId);
            AssertSessionResultData(expected.ScoredSessionResults.First(), test.SessionResults.First());
        }

        private void AssertSessionResultData(ScoredSessionResultEntity expected, ResultModel test)
        {
            Assert.Equal(expected.ScoringId, test.ScoringId);
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
