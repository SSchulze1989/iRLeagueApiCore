using FluentValidation.TestHelper;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Server.Validation.Sessions;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Validators.Sessions
{
    [Collection("ValidatorTests")]
    public class PostSessionToScheduleValidatorTests : IClassFixture<DbTestFixture>
    {
        private readonly DbTestFixture fixture;

        private const long testLeagueId = 1;
        private const long testScheduleId = 1;
        private const long testSessionId = 1;
        private const string testSessionName = "TestSession";
        private const string testSubSessionName = "TestSubSession";

        public PostSessionToScheduleValidatorTests(DbTestFixture fixture)
        {
            this.fixture = fixture;
        }

        private static PostSessionToScheduleRequest DefaultRequest(long leagueId = testLeagueId, long scheduleId = testScheduleId)
        {
            var model = new PostSessionModel()
            {
                Name = testSessionName,
                SubSessions = new List<PutSessionSubSessionModel>()
                {
                    new PutSessionSubSessionModel()
                    {
                        Name = testSubSessionName,
                        SubSessionNr = 1,
                    }
                }
            };
            return new PostSessionToScheduleRequest(leagueId, scheduleId, LeagueUser.Empty, model);
        }

        private static PostSessionToScheduleRequestValidator CreateValidator(LeagueDbContext dbContext)
        {
            return new PostSessionToScheduleRequestValidator(dbContext);
        }

        [Fact]
        public async Task ShouldValidateSubSessionNotNull()
        {
            using var dbContext = fixture.CreateDbContext();
            var request = DefaultRequest();
            request.Model.SubSessions = null;
            var validator = CreateValidator(dbContext);
            var result = await validator.TestValidateAsync(request);
            result.ShouldHaveValidationErrorFor(x => x.Model.SubSessions);
        }

        [Theory]
        [InlineData(0, true)]
        [InlineData(1, false)]
        [InlineData(42, false)]
        public async Task ShouldValidateSubSessionIdIsZero(long subSessionId, bool expectValid)
        {
            using var dbContext = fixture.CreateDbContext();
            var request = DefaultRequest();
            request.Model.SubSessions.First().SubSessionId = subSessionId;
            var validator = CreateValidator(dbContext);
            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.Model.SubSessions);
            }
        }

        [Theory]
        [InlineData(testLeagueId, testScheduleId, true)]
        [InlineData(0, testScheduleId, false)]
        [InlineData(testLeagueId, 0, false)]
        [InlineData(42, testScheduleId, false)]
        [InlineData(testLeagueId, 42, false)]
        public async Task ShoulValidateScheduleExists(long leagueId, long scheduleId, bool expectValid)
        {
            using var dbContext = fixture.CreateDbContext();
            var request = DefaultRequest(leagueId, scheduleId);
            var validator = CreateValidator(dbContext);
            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.ScheduleId);
            }
        }
    }
}
