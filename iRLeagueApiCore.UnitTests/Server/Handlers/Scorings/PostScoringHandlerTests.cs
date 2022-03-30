using FluentValidation;
using FluentValidation.Results;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.AspNetCore.Identity.Test;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings
{
    public class PostScoringHandlerTests : HandlersTestsBase<PostScoringHandler, PostScoringRequest, GetScoringModel>, IClassFixture<DbTestFixture>
    { 
        public PostScoringHandlerTests(DbTestFixture fixture) : base(fixture)
        { }

        protected override PostScoringRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testScoringId);
        }

        protected PostScoringRequest DefaultRequest(long leagueId, long seasonId)
        {
            var model = new PostScoringModel()
            {
                BasePoints = new double[0],
                BonusPoints = new string[0]
            };
            return new PostScoringRequest(leagueId, seasonId, model);
        }

        protected override PostScoringHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostScoringRequest> validator)
        {
            return new PostScoringHandler(logger, dbContext, new IValidator<PostScoringRequest>[] { validator });
        }

        protected override void DefaultAssertions(PostScoringRequest request, GetScoringModel result, LeagueDbContext dbContext)
        {
            base.DefaultAssertions(request, result, dbContext);
            Assert.NotEqual(0, result.Id);
            Assert.Contains(dbContext.Scorings, x => x.ScoringId == result.Id);
            Assert.Empty(result.BasePoints);
            Assert.Empty(result.BonusPoints);
        }

        [Fact]
        public override async Task<GetScoringModel> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Theory]
        [InlineData(0,testSeasonId)]
        [InlineData(testLeagueId,0)]
        [InlineData(42,testSeasonId)]
        [InlineData(testLeagueId,42)]
        public async Task HandleNotFoundAsync(long leagueId, long seasonId)
        {
            var request = DefaultRequest(leagueId, seasonId);
            await base.HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }
    }
}
