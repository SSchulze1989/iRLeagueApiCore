using iRLeagueApiCore.Common.Enums;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess
{
    public class DataAccessMockHelper
    {
        private readonly Fixture fixture = new();

        public DataAccessMockHelper()
        {
            using var dbContext = CreateMockDbContext();
            dbContext.Database.EnsureDeleted();
            dbContext.Database.EnsureCreated();
        }

        public LeagueDbContext CreateMockDbContext()
        {
            var optionsBuilder = new DbContextOptionsBuilder<LeagueDbContext>();
            optionsBuilder.UseInMemoryDatabase(databaseName: "TestDatabase")
               .UseLazyLoadingProxies();
            var dbContext = new LeagueDbContext(optionsBuilder.Options);

            return dbContext;
        }

        public async Task PopulateBasicTestSet(LeagueDbContext dbContext)
        {
            var members = CreateMembers();
            dbContext.Members.AddRange(members);

            var league = CreateLeague();
            dbContext.Leagues.Add(league);

            var season = CreateSeason(league);
            league.Seasons.Add(season);

            var leagueMembers = CreateLeagueMembers(league, members);
            league.LeagueMembers = leagueMembers.ToList();

            var teams = CreateTeams(league, leagueMembers);
            league.Teams = teams.ToList();

            var schedule = CreateSchedule(season);
            season.Schedules.Add(schedule);

            var events = CreateSessions(schedule);
            schedule.Events = events.ToList();

            foreach (var @event in events)
            {
                var result = CreateResult(@event, leagueMembers);
                @event.EventResult = result;
            }

            await dbContext.SaveChangesAsync();
        }

        private LeagueEntity CreateLeague()
        {
            return fixture.Build<LeagueEntity>()
                .Without(x => x.LeagueMembers)
                .Without(x => x.PointRules)
                .Without(x => x.ResultConfigs)
                .Without(x => x.Scorings)
                .Without(x => x.Seasons)
                .Without(x => x.Teams)
                .Create();
        }

        private SeasonEntity CreateSeason(LeagueEntity league)
        {
            return fixture.Build<SeasonEntity>()
                .With(x => x.League, league)
                .With(x => x.Finished, false)
                .Without(x => x.MainScoring)
                .Without(x => x.Schedules)
                .Without(x => x.Standings)
                .Without(x => x.StatisticSets)
                .Create();
        }

        private IEnumerable<MemberEntity> CreateMembers(int count = 10)
        {
            return fixture.Build<MemberEntity>()
                .Without(x => x.AcceptedReviewVotes)
                .Without(x => x.CleanestDriverResults)
                .Without(x => x.CommentReviewVotes)
                .Without(x => x.DriverStatisticRows)
                .Without(x => x.FastestAvgLapResults)
                .Without(x => x.FastestLapResults)
                .Without(x => x.FastestQualyLapResults)
                .Without(x => x.HardChargerResults)
                .Without(x => x.InvolvedReviews)
                .Without(x => x.ResultRows)
                .Without(x => x.StatisticSets)
                .CreateMany(count)
                .ToList();
        }

        private IEnumerable<LeagueMemberEntity> CreateLeagueMembers(LeagueEntity league, IEnumerable<MemberEntity> members)
        {
            return members.Select(member => fixture.Build<LeagueMemberEntity>()
                .With(x => x.League, league)
                .With(x => x.Member, member)
                .Without(x => x.Team)
                .Without(x => x.TeamId)
                .Create())
                .ToList();
        }

        private IEnumerable<TeamEntity> CreateTeams(LeagueEntity league, IEnumerable<LeagueMemberEntity> leagueMembers)
        {
            var memberChunks = leagueMembers.Chunk(3);
            var teams = memberChunks
                .Select(members => fixture.Build<TeamEntity>()
                    .With(x => x.League, league)
                    .With(x => x.Members, members.Take(2).ToList())
                    .Create())
                .ToList();

            return teams;
        }

        private ScheduleEntity CreateSchedule(SeasonEntity season)
        {
            return fixture.Build<ScheduleEntity>()
                .With(x => x.Season, season)
                .Without(x => x.Events)
                .Without(x => x.Scorings)
                .Create();
        }

        private IEnumerable<EventEntity> CreateSessions(ScheduleEntity schedule)
        {
            var sessions = () => fixture.Build<SessionEntity>()
                .Without(x => x.Event)
                .With(x => x.SessionType, SessionType.Race)
                .Without(x => x.IncidentReviews)
                .Without(x => x.SessionResult)
                .CreateMany();
            return fixture.Build<EventEntity>()
                .With(x => x.Schedule, schedule)
                .With(x => x.Sessions, sessions().ToList())
                .Without(x => x.EventResult)
                .Without(x => x.ResultConfigs)
                .Without(x => x.ScoredEventResults)
                .Without(x => x.Track)
                .CreateMany()
                .ToList();
        }

        private EventResultEntity CreateResult(EventEntity @event, IEnumerable<LeagueMemberEntity> members)
        {
            return fixture.Build<EventResultEntity>()
                .With(x => x.Event, @event)
                .With(x => x.SessionResults, CreateSessionResults(@event, members).ToList())
                .Without(x => x.ScoredResults)
                .Create();
        }

        private IEnumerable<SessionResultEntity> CreateSessionResults(EventEntity @event, IEnumerable<LeagueMemberEntity> members)
        {
            return @event.Sessions.Select(session => fixture.Build<SessionResultEntity>()
                .With(x => x.Session, session)
                .With(x => x.ResultRows, () => members.Select(member => fixture.Build<ResultRowEntity>()
                    .With(x => x.Member, member.Member)
                    .With(x => x.LeagueMember, member)
                    .Without(x => x.Team)
                    .Without(x => x.SubResult)
                    .Create()).ToList())
                .Without(x => x.IRSimSessionDetails)
                .Without(x => x.Result)
                .Create())
                .ToList();
        }
    }
}
