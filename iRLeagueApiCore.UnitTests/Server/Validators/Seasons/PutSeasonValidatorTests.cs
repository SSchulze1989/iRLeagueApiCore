using FluentValidation.TestHelper;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Seasons;
using iRLeagueApiCore.Server.Models;
using iRLeagueApiCore.Server.Validation.Seasons;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Validators.Seasons
{
    [Collection("ValidatorTests")]
    public class PutSeasonValidatorTests : IClassFixture<DbTestFixture>
    {
        private readonly DbTestFixture fixture;

        private const long testLeagueId = 1;
        private const long testSeasonId = 1;
        private const string testSeasonName = "TestSeason";

        public PutSeasonValidatorTests(DbTestFixture fixture)
        {
            this.fixture = fixture;
        }

        private static PutSeasonRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            var model = new PutSeasonModel()
            {
                HideComments = true,
                MainScoringId = null,
                Finished = true,
                SeasonName = testSeasonName,
            };
            return new PutSeasonRequest(leagueId, LeagueUser.Empty, seasonId, model);
        }

        private static PutSeasonRequestValidator CreateValidator(LeagueDbContext dbContext)
        {
            return new PutSeasonRequestValidator(dbContext, new PutSeasonModelValidator());
        }

        [Theory]
        [InlineData(1, 1, true)]
        [InlineData(1, null, true)]
        [InlineData(2, 1, false)]
        [InlineData(1, 42, false)]
        public async Task ValidateMainScoring(long leagueId, long? mainScoringId, bool expectValid)
        {
            var dbContext = fixture.CreateDbContext();
            var request = DefaultRequest(leagueId);
            request.Model.MainScoringId = mainScoringId;
            var validator = CreateValidator(dbContext);
            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.Model.MainScoringId);
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task ValidateLeagueId(long leagueId, bool expectValid)
        {
            var dbContext = fixture.CreateDbContext();
            var request = DefaultRequest(leagueId);
            var validator = CreateValidator(dbContext);
            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.LeagueId);
            }
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(0, false)]
        public async Task ValidateSeasonId(long seasonId, bool expectValid)
        {
            var dbContext = fixture.CreateDbContext();
            var request = DefaultRequest(seasonId: seasonId);
            var validator = CreateValidator(dbContext);
            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.SeasonId);
            }
        }

        [Theory]
        [InlineData("ValidName", true)]
        [InlineData("", false)]
        [InlineData(null, false)]
        public async Task ValidateSeasonName(string name, bool expectValid)
        {
            var dbContext = fixture.CreateDbContext();
            var request = DefaultRequest();
            request.Model.SeasonName = name;
            var validator = CreateValidator(dbContext);
            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.Model.SeasonName);
            }
        }
    }
}
