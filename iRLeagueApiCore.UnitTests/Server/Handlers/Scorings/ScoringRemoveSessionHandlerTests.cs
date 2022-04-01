using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Scorings;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Scorings
{
    public class ScoringRemoveSessionHandlerTests : HandlersTestsBase<ScoringRemoveSessionHandler, ScoringRemoveSessionRequest, Unit>
    {
        private new const long testScoringId = 1;
        private const long testSessionId = 1;

        public ScoringRemoveSessionHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        protected override ScoringRemoveSessionHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<ScoringRemoveSessionRequest> validator)
        {
            return new ScoringRemoveSessionHandler(dbContext, new IValidator<ScoringRemoveSessionRequest>[] { validator });
        }

        protected override void DefaultPreTestAssertions(ScoringRemoveSessionRequest request, LeagueDbContext dbContext)
        {
            var scoring = dbContext.Scorings
                .Include(x => x.Sessions)
                .SingleOrDefault(x => x.ScoringId == request.ScoringId);
            Assert.Contains(scoring.Sessions, x => x.SessionId == request.SessionId);
        }

        protected override ScoringRemoveSessionRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testScoringId, testSessionId);
        }

        private ScoringRemoveSessionRequest DefaultRequest(long leagueId, long scoringId, long sessionId)
        {
            return new ScoringRemoveSessionRequest(leagueId, scoringId, sessionId);
        }

        protected override void DefaultAssertions(ScoringRemoveSessionRequest request, Unit result, LeagueDbContext dbContext)
        {
            // Session should not be in scoring anymore
            var scoring = dbContext.Scorings
                .Include(x => x.Sessions)
                .SingleOrDefault(x => x.ScoringId == request.ScoringId);
            Assert.DoesNotContain(scoring.Sessions, x => x.SessionId == request.SessionId);
        }

        [Fact]
        public override async Task<Unit> HandleDefaultAsync()
        {
            return await base.HandleDefaultAsync();
        }

        [Fact]
        public override async Task HandleValidationFailedAsync()
        {
            await base.HandleValidationFailedAsync();
        }

        [Theory]
        [InlineData(0, testScoringId, testSessionId)]
        [InlineData(testLeagueId, 0, testSessionId)]
        [InlineData(testLeagueId, testScoringId, 0)]
        [InlineData(42, testScoringId, testSessionId)]
        [InlineData(testLeagueId, 42, testSessionId)]
        [InlineData(testLeagueId, testScoringId, 42)]
        public async Task HandleNotFound(long leagueId, long scoringId, long sessionId)
        {
            var request = DefaultRequest(leagueId, scoringId, sessionId);
            await HandleNotFoundRequestAsync(request);
        }
    }
}
