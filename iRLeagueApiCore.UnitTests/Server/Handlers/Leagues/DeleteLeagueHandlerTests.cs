using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Leagues
{
    [Collection("HandlerTests")]
    public class DeleteLeagueHandlerTests : HandlersTestsBase<DeleteLeagueHandler, DeleteLeagueRequest, Unit>
    {
        public DeleteLeagueHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override DeleteLeagueHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteLeagueRequest> validator)
        {
            return new DeleteLeagueHandler(logger, dbContext, new IValidator<DeleteLeagueRequest>[] { validator });
        }

        protected override DeleteLeagueRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId);
        }

        protected override void DefaultPreTestAssertions(DeleteLeagueRequest request, LeagueDbContext dbContext)
        {
            Assert.Contains(dbContext.Leagues, x => x.Id == request.LeagueId);
        }

        protected override void DefaultAssertions(DeleteLeagueRequest request, Unit result, LeagueDbContext dbContext)
        {
            Assert.DoesNotContain(dbContext.Leagues, x => x.Id == request.LeagueId);
        }

        private DeleteLeagueRequest DefaultRequest(long leagueId)
        {
            return new DeleteLeagueRequest(leagueId);
        }

        [Fact]
        public async override Task<Unit> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public async override Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(42)]
        public async Task HandleNotFoundAsync(long leagueId)
        {
            var request = DefaultRequest(leagueId);
            await base.HandleNotFoundRequestAsync(request);
        }
    }
}
