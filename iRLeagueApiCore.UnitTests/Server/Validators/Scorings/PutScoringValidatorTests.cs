using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.Server.Validation.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Validators.Scorings
{
    public class PutScoringValidatorTests : IClassFixture<DbTestFixture>
    {
        private readonly DbTestFixture fixture;

        private const long testLeagueId = 1;
        private const long testScoringId = 1;

        public PutScoringValidatorTests(DbTestFixture fixture)
        {
            this.fixture = fixture;
        }

        private static PutScoringRequest DefaultRequest(long leagueId, long scoringId)
        {
            return new PutScoringRequest(leagueId, scoringId)
            {
                BasePoints = new double[0],
                BonusPoints = new string[0]
            };
        }

        [Theory]
        [InlineData(1, true)]
        [InlineData(42, false)]
        [InlineData(0, false)]
        public async Task ValidateLeagueId(long leagueId, bool expectValid)
        {
            using (var dbContext = fixture.CreateDbContext())
            {
                var validator = new PutScoringValidator(dbContext);
                var request = DefaultRequest(leagueId, testScoringId);

                var result = await validator.ValidateAsync(request);
                Assert.Equal(expectValid, result.IsValid);
            }
        }

        [Theory]
        [InlineData(new double[] { 10, 8 }, true)]
        [InlineData(new double[0], true)]
        [InlineData(null, false)]
        public async Task ValidateBasePoints(IEnumerable<double> points, bool expectValid)
        {
            using (var dbContext = fixture.CreateDbContext())
            {
                var validator = new PutScoringValidator(dbContext);
                var request = DefaultRequest(testLeagueId, testScoringId);
                request.BasePoints = points;

                var result = await validator.ValidateAsync(request);
                if (expectValid)
                {
                    Assert.True(result.IsValid);
                }
                else
                {
                    Assert.False(result.IsValid);
                    Assert.Contains(result.Errors, x => x.PropertyName == nameof(PutScoringRequest.BasePoints));
                }
            }
        }

        [Theory]
        [InlineData(new string[] { "p1:3", "p2:2", "q1:1" }, true)]
        [InlineData(new string[0], true)]
        [InlineData(new string[] { "p1:3", "p:2" }, false)]
        [InlineData(new string[] { "p1:3", "p2:" }, false)]
        [InlineData(new string[] { "p1:3", "a2:2" }, false)]
        [InlineData(null, false)]
        public async Task ValidateBonusPoints(IEnumerable<string> bonusPoints, bool expectValid)
        {
            using (var dbContest = fixture.CreateDbContext())
            {
                var validator = new PutScoringValidator(dbContest);
                var request = DefaultRequest(testLeagueId, testScoringId);
                request.BonusPoints = bonusPoints;

                var result = await validator.ValidateAsync(request);
                if (expectValid)
                {
                    Assert.True(result.IsValid);
                }
                else
                {
                    Assert.False(result.IsValid);
                    Assert.Contains(result.Errors, x => x.PropertyName == nameof(PutScoringRequest.BonusPoints));
                }
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
            using (var dbContext = fixture.CreateDbContext())
            {
                var validator = new PutScoringValidator(dbContext);
                var request = DefaultRequest(testLeagueId, testScoringId);
                request.ExtScoringSourceId = scoringId;
                request.TakeResultsFromExtSource = useExtScoring;

                var result = await validator.ValidateAsync(request);
                if (expectValid)
                {
                    Assert.True(result.IsValid);
                }
                else
                {
                    Assert.False(result.IsValid);
                    Assert.Contains(result.Errors, x => x.PropertyName == nameof(PutScoringRequest.ExtScoringSourceId));
                }
            }
        }
    }
}
