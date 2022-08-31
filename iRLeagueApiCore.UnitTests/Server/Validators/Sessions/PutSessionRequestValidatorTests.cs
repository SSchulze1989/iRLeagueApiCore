using FluentValidation.TestHelper;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Server.Validation.Events;
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
    public class PutSessionRequestValidatorTests : IClassFixture<DbTestFixture>
    {
        private readonly DbTestFixture fixture;

        private const long testLeagueId = 1;
        private const long testSessionId = 1;
        private const string testSessionName = "TestSession";
        private const string testSubSessionName = "TestSubSession";

        public PutSessionRequestValidatorTests(DbTestFixture fixture)
        {
            this.fixture = fixture;
        }

        private static PutSessionRequest DefaultRequest(long leagueId = testLeagueId, long sessionId = testSessionId)
        {
            var model = new PutSessionModel()
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
            return new PutSessionRequest(leagueId, LeagueUser.Empty, sessionId, model);
        }

        private static PutEventRequestValidator CreateValidator(LeagueDbContext dbContext)
        {
            return new PutEventRequestValidator(dbContext);
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
        [InlineData(1, true)]
        [InlineData(42, false)]
        public async Task ShouldValidateSubSessionIdExists(long subSessionId, bool expectValid)
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
    }
}
