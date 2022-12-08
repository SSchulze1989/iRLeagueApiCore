using FluentAssertions;
using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Leagues
{
    [Collection("DbTestFixture")]
    public class PutLeagueDbTestFixture : HandlersTestsBase<PutLeagueHandler, PutLeagueRequest, LeagueModel>
    {
        public PutLeagueDbTestFixture(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override PutLeagueHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutLeagueRequest> validator)
        {
            return new PutLeagueHandler(logger, dbContext, new IValidator<PutLeagueRequest>[] { validator });
        }

        protected override PutLeagueRequest DefaultRequest()
        {
            return DefaultRequest();
        }

        public PutLeagueRequest DefaultRequest(long leagueId = testLeagueId)
        {
            var model = new PutLeagueModel()
            {
                NameFull = "Put league test"
            };
            return new PutLeagueRequest(leagueId, DefaultUser(), model);
        }

        protected override void DefaultAssertions(PutLeagueRequest request, LeagueModel result, LeagueDbContext dbContext)
        {
            var expected = request.Model;
            result.Id.Should().Be(request.LeagueId);
            result.NameFull.Should().Be(expected.NameFull);
            AssertChanged(request.User, DateTime.UtcNow, result);
            base.DefaultAssertions(request, result, dbContext);
        }

        [Fact]
        public override async Task<LeagueModel> ShouldHandleDefault()
        {
            return await base.ShouldHandleDefault();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public async Task HandleNotFoundAsync(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await base.HandleNotFoundRequestAsync(request);
        }

        [Fact]
        public async override Task ShouldHandleValidationFailed()
        {
            await base.ShouldHandleValidationFailed();
        }
    }
}
