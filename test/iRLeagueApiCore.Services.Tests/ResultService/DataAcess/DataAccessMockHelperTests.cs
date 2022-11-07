using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess
{
    [Collection("DataAccessTests")]
    public class DataAccessMockHelperTests
    {
        private readonly DataAccessMockHelper accessMockHelper = new();

        [Fact]
        public async Task PopulateBasicTestsSet_ShouldNotThrow()
        {
            using var dbContext = accessMockHelper.CreateMockDbContext();
            await accessMockHelper.PopulateBasicTestSet(dbContext);
        }

        [Fact]
        public async Task PopulateBasicTestSet_ShouldNotLoseDataBetweenContexts()
        {
            using (var dbContext = accessMockHelper.CreateMockDbContext())
            {
                await accessMockHelper.PopulateBasicTestSet(dbContext);
            }

            using (var dbContext = accessMockHelper.CreateMockDbContext())
            {
                dbContext.Leagues.Should().HaveCount(1);
            }
        }

        [Fact]
        public async Task PopulateBasicTestSet_ShouldHaveCorrectNavigationProperties()
        {
            using var dbContext = accessMockHelper.CreateMockDbContext();
            await accessMockHelper.PopulateBasicTestSet(dbContext);

            var session = await dbContext.Sessions
                .Include(x => x.Event)
                .FirstAsync();
            var @event = session.Event;

            session.Event.Should().NotBeNull();
            @event.Sessions.Should().Contain(session);
        }

        [Fact]
        public async Task PopulateBasicTestSet_EventsShouldHaveRawResults()
        {
            using var dbContext = accessMockHelper.CreateMockDbContext();
            await accessMockHelper.PopulateBasicTestSet(dbContext);

            var events = await dbContext.Events
                .Include(x => x.EventResult)
                .ToListAsync();

            foreach(var @event in events)
            {
                @event.EventResult.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task PopulateBasicTestSet_SomeLeagueMembersShouldHaveTeam()
        {
            using var dbContext = accessMockHelper.CreateMockDbContext();
            await accessMockHelper.PopulateBasicTestSet(dbContext);

            var leagueMembers = await dbContext.LeagueMembers
                .Include(x => x.Team)
                .ToListAsync();

            leagueMembers.Where(x => x.Team != null).Should().HaveCountGreaterThan(0);
        }
    }
}
