
using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueApiCore.Server.Handlers.Sessions;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Sessions
{
    [Collection("HandlerTests")]
    public class DeleteSessionHandlerTests : HandlersTestsBase<DeleteSessionHandler, DeleteSessionRequest, Unit>
    {
        private const long testSessionId = 1;
        private const string testSessionName = "TestSession";

        public DeleteSessionHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override DeleteSessionHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteSessionRequest> validator)
        {
            return new DeleteSessionHandler(logger, dbContext, new IValidator<DeleteSessionRequest>[] { validator });
        }

        protected override DeleteSessionRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testSessionId);
        }

        private DeleteSessionRequest DefaultRequest(long leagueId = testLeagueId, long SessionId = testSessionId)
        {

            return new DeleteSessionRequest(leagueId, SessionId);
        }

        protected override void DefaultPreTestAssertions(DeleteSessionRequest request, LeagueDbContext dbContext)
        {
            Assert.Contains(dbContext.Sessions, x => x.SessionId == request.SessionId);
        }

        protected override void DefaultAssertions(DeleteSessionRequest request, Unit result, LeagueDbContext dbContext)
        {
            Assert.DoesNotContain(dbContext.Sessions, x => x.SessionId == request.SessionId);
        }

        [Fact]
        public async override Task<Unit> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, 1)]
        [InlineData(1, 0)]
        [InlineData(42, 1)]
        [InlineData(1, 42)]
        public async Task HandleNotFound(long leagueId, long SessionId)
        {
            var request = DefaultRequest(leagueId, SessionId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
