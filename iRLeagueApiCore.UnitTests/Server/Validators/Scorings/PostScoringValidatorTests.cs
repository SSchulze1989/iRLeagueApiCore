using FluentValidation.TestHelper;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.Server.Validation.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Validators.Scorings
{
    public class PostScoringValidatorTests : IClassFixture<DbTestFixture>
    {
        private readonly DbTestFixture fixture;

        private const long testLeagueId = 1;
        private const long testSeasonId = 1;

        public PostScoringValidatorTests(DbTestFixture fixture)
        {
            this.fixture = fixture;
        }

        private static PostScoringRequest DefaultRequest(long leagueId = testLeagueId, long seasonId = testSeasonId)
        {
            var model = new PostScoringModel()
            {
                BasePoints = new double[0],
                BonusPoints = new string[0]
            };
            return new PostScoringRequest(leagueId, seasonId, model);
        }

        private static PostScoringRequestValidator CreateValidator(LeagueDbContext dbContext)
        {
            return new PostScoringRequestValidator(dbContext, new PostScoringModelValidator(dbContext));
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(42, false)]
        [InlineData(0, false)]
        public async Task ValidateLeagueId(long leagueId, bool expectValid)
        {
            using var dbContext = fixture.CreateDbContext();
            
            var validator = CreateValidator(dbContext);
            var request = DefaultRequest(leagueId: leagueId);

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
        [InlineData(42, false)]
        public async Task ValidateSeasonId(long seasonId, bool expectValid)
        {
            using var dbContext = fixture.CreateDbContext();
            
            var validator = CreateValidator(dbContext);
            var request = DefaultRequest(seasonId: seasonId);

            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.SeasonId);
            }
        }

        [Theory]
        [InlineData(new double[] { 10, 8}, true)]
        [InlineData(new double[0], true)]
        [InlineData(null, false)]
        public async Task ValidateBasePoints(IEnumerable<double> points, bool expectValid)
        {
            using var dbContext = fixture.CreateDbContext();
            
            var validator = CreateValidator(dbContext);
            var request = DefaultRequest();
            request.Model.BasePoints = points;

            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.Model.BasePoints);
            }
        }

        [Theory]
        [InlineData(new string[] {"p1:3", "p2:2", "q1:1"}, true)]
        [InlineData(new string[0], true)]
        [InlineData(new string[] { "p1:3", "p:2" }, false)]
        [InlineData(new string[] { "p1:3", "p2:"}, false)]
        [InlineData(new string[] { "p1:3", "a2:2" }, false)]
        [InlineData(null, false)]
        public async Task ValidateBonusPoints(IEnumerable<string> bonusPoints, bool expectValid)
        {
            using var dbContext = fixture.CreateDbContext();
            
            var validator = CreateValidator(dbContext);
            var request = DefaultRequest();
            request.Model.BonusPoints = bonusPoints;

            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.Model.BonusPoints);
            }
        }

        [Theory]
        [InlineData(1, true, true)]
        [InlineData(null, false, true)]
        [InlineData(0, false, true)]
        [InlineData(0, true, false)]
        [InlineData(42, true, false)]
        public async Task ValidateExtScoringSource(long? scoringId, bool useExtScoring, bool expectValid)
        {
            using var dbContext = fixture.CreateDbContext();

            var validator = CreateValidator(dbContext);
            var request = DefaultRequest();
            request.Model.ExtScoringSourceId = scoringId;
            request.Model.TakeResultsFromExtSource = useExtScoring;

            var result = await validator.TestValidateAsync(request);
            Assert.Equal(expectValid, result.IsValid);
            if (expectValid == false)
            {
                result.ShouldHaveValidationErrorFor(x => x.Model.ExtScoringSourceId);
            }
        }
    }
}
