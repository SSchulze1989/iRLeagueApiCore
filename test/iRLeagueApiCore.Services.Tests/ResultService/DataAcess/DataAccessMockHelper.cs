﻿using AutoFixture.Dsl;
using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Extensions;
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

            var events = CreateEvents(schedule);
            schedule.Events = events.ToList();

            foreach (var @event in events)
            {
                var result = CreateResult(@event, leagueMembers);
                dbContext.EventResults.Add(result);

                var reviews = CreateReviews(@event);
                dbContext.IncidentReviews.AddRange(reviews);
            }

            var voteCategories = CreateVoteCategories(league);
            league.VoteCategories = voteCategories.ToList();

            await dbContext.SaveChangesAsync();
        }

        public LeagueEntity CreateLeague()
        {
            return fixture.Build<LeagueEntity>()
                .Without(x => x.VoteCategories)
                .Without(x => x.LeagueMembers)
                .Without(x => x.PointRules)
                .Without(x => x.ResultConfigs)
                .Without(x => x.Scorings)
                .Without(x => x.Seasons)
                .Without(x => x.Teams)
                .Create();
        }

        public SeasonEntity CreateSeason(LeagueEntity league)
        {
            return fixture.Build<SeasonEntity>()
                .With(x => x.League, league)
                .With(x => x.LeagueId, league.Id)
                .With(x => x.Finished, false)
                .Without(x => x.MainScoring)
                .Without(x => x.Schedules)
                .Without(x => x.Standings)
                .Without(x => x.StatisticSets)
                .Create();
        }

        public IEnumerable<MemberEntity> CreateMembers(int count = 10)
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

        public IEnumerable<LeagueMemberEntity> CreateLeagueMembers(LeagueEntity league, IEnumerable<MemberEntity> members)
        {
            return members.Select(member => fixture.Build<LeagueMemberEntity>()
                .With(x => x.League, league)
                .With(x => x.LeagueId, league.Id)
                .With(x => x.Member, member)
                .Without(x => x.Team)
                .Without(x => x.TeamId)
                .Create())
                .ToList();
        }

        public IEnumerable<TeamEntity> CreateTeams(LeagueEntity league, IEnumerable<LeagueMemberEntity> leagueMembers)
        {
            var memberChunks = leagueMembers.Chunk(3);
            var teams = memberChunks
                .Select(members => fixture.Build<TeamEntity>()
                    .With(x => x.League, league)
                    .With(x => x.LeagueId, league.Id)
                    .With(x => x.Members, members.Take(2).ToList())
                    .Create())
                .ToList();

            return teams;
        }

        public ScheduleEntity CreateSchedule(SeasonEntity season)
        {
            return fixture.Build<ScheduleEntity>()
                .With(x => x.LeagueId, season.LeagueId)
                .With(x => x.Season, season)
                .Without(x => x.Events)
                .Without(x => x.Scorings)
                .Create();
        }

        public SessionEntity CreateSession(EventEntity @event, SessionType sessionType = SessionType.Race)
        {
            return fixture.Build<SessionEntity>()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.EventId, @event.EventId)
                .With(x => x.SessionType, sessionType)
                .Without(x => x.IncidentReviews)
                .Without(x => x.SessionResult)
                .Create();
        }

        public IPostprocessComposer<EventEntity> EventBuilder(ScheduleEntity schedule)
        {
            var sessions = () => fixture.Build<SessionEntity>()
                .With(x => x.LeagueId, schedule.LeagueId)
                .With(x => x.SessionType, SessionType.Race)
                .Without(x => x.Event)
                .Without(x => x.EventId)
                .Without(x => x.IncidentReviews)
                .Without(x => x.SessionResult)
                .CreateMany();
            return fixture.Build<EventEntity>()
                .With(x => x.LeagueId, schedule.LeagueId)
                .With(x => x.Schedule, schedule)
                .With(x => x.Sessions, () => sessions().ToList())
                .Without(x => x.EventResult)
                .Without(x => x.ResultConfigs)
                .Without(x => x.ScoredEventResults)
                .Without(x => x.Track)
                .Without(x => x.SimSessionDetails);
        }

        public IEnumerable<EventEntity> CreateEvents(ScheduleEntity schedule)
        {
            return EventBuilder(schedule)
                .CreateMany()
                .ToList();
        }

        public EventResultEntity CreateResult(EventEntity @event, IEnumerable<LeagueMemberEntity> members)
        {
            return fixture.Build<EventResultEntity>()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.EventId, @event.EventId)
                .With(x => x.SessionResults, CreateSessionResults(@event, members).ToList())
                .Without(x => x.ScoredResults)
                .Create();
        }

        public IEnumerable<SessionResultEntity> CreateSessionResults(EventEntity @event, IEnumerable<LeagueMemberEntity> members)
        {
            return @event.Sessions.Select(session => fixture.Build<SessionResultEntity>()
                .With(x => x.LeagueId, session.LeagueId)
                .With(x => x.EventId, @event.EventId)
                .With(x => x.Session, session)
                .With(x => x.ResultRows, () => members.Select(member => fixture.Build<ResultRowEntity>()
                    .With(x => x.LeagueId, session.LeagueId)
                    .With(x => x.Member, member.Member)
                    .With(x => x.LeagueMember, member)
                    .With(x => x.Team, member.Team)
                    .With(x => x.TeamId, member.TeamId)
                    .Without(x => x.SubResult)
                    .Create()).ToList())
                .Without(x => x.IRSimSessionDetails)
                .Without(x => x.Result)
                .Create())
                .ToList();
        }

        public IPostprocessComposer<ResultConfigurationEntity> ConfigurationBuilder(EventEntity @event)
        {
            return fixture.Build<ResultConfigurationEntity>()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Events, new[] { @event })
                .With(x => x.Scorings, @event.Sessions
                    .Where(x => x.SessionType == SessionType.Race)
                    .Select(session => fixture.Build<ScoringEntity>()
                        .With(x => x.LeagueId, @event.LeagueId)
                        .With(x => x.Index, session.SessionNr)
                        .Without(x => x.PointsRule)
                        .Without(x => x.DependendScorings)
                        .Without(x => x.ExtScoringSource)
                        .Without(x => x.ResultConfiguration)
                        .Create())
                    .ToList())
                .Without(x => x.League)
                .Without(x => x.StandingConfigurations)
                .Without(x => x.PointFilters)
                .Without(x => x.ResultFilters)
                .Without(x => x.SourceResultConfigId)
                .Without(x => x.SourceResultConfig);
        }

        public ResultConfigurationEntity CreateConfiguration(EventEntity @event)
        {
            return ConfigurationBuilder(@event)
                .Create();
        }

        public StandingConfigurationEntity CreateStandingConfiguration(ResultConfigurationEntity? resultConfig)
        {
            return fixture.Build<StandingConfigurationEntity>()
                .With(x => x.ResultConfigurations, (new[] { resultConfig }).NotNull().ToList())
                .Without(x => x.Standings)
                .Create();
        }

        public ScoredEventResultEntity CreateScoredResult(EventEntity @event, ResultConfigurationEntity? config)
        {
            return fixture.Build<ScoredEventResultEntity>()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Event, @event)
                .With(x => x.ScoredSessionResults, @event.Sessions.Select(session => fixture.Build<ScoredSessionResultEntity>()
                    .With(x => x.LeagueId, session.LeagueId)
                    .With(x => x.Name, session.Name)
                    .With(x => x.SessionNr, session.SessionNr)
                    .With(x => x.ScoredResultRows, CreateScoredResultRows(session))
                    .Without(x => x.Scoring)
                    .Without(x => x.ScoredEventResult)
                    .Without(x => x.CleanestDrivers)
                    .Without(x => x.FastestAvgLapDriver)
                    .Without(x => x.FastestLapDriver)
                    .Without(x => x.FastestQualyLapDriver)
                    .Without(x => x.HardChargers)
                    .Create())
                    .ToList())
                .With(x => x.ResultConfig, config)
                .With(x => x.ResultConfigId, config?.ResultConfigId)
                .Create();
        }

        public IEnumerable<ScoredResultRowEntity> CreateScoredResultRows(SessionEntity session)
        {
            return session.SessionResult.ResultRows.Select(row => fixture.Build<ScoredResultRowEntity>()
                .With(x => x.LeagueId, row.LeagueId)
                .With(x => x.Member, row.Member)
                .With(x => x.MemberId, row.MemberId)
                .With(x => x.Team, row.Team)
                .With(x => x.TeamId, row.TeamId)
                .Without(x => x.AddPenalty)
                .Without(x => x.ReviewPenalties)
                .Without(x => x.ScoredSessionResult)
                .Without(x => x.TeamResultRows)
                .Without(x => x.TeamParentRows)
                .Without(x => x.StandingRows)
                .Create()).ToList();
        }

        public PointRuleEntity CreatePointRule(LeagueEntity league)
        {
            return fixture.Build<PointRuleEntity>()
                .With(x => x.League, league)
                .With(x => x.LeagueId, league.Id)
                .Without(x => x.Scorings)
                .Create();
        }

        public IEnumerable<IncidentReviewEntity> CreateReviews(EventEntity @event)
        {
            return @event.Sessions.Select(session => fixture.Build<IncidentReviewEntity>()
                .With(x => x.LeagueId, @event.LeagueId)
                .With(x => x.Session, session)
                .With(x => x.SessionId, session.SessionId)
                .Without(x => x.AcceptedReviewVotes)
                .Without(x => x.Comments)
                .Without(x => x.InvolvedMembers)
                .Without(x => x.ReviewPenaltys)
                .Create()).ToList();
        }

        public IPostprocessComposer<AcceptedReviewVoteEntity> AcceptedReviewVoteBuilder()
        {
            return fixture.Build<AcceptedReviewVoteEntity>()
                .Without(x => x.MemberAtFault)
                .Without(x => x.Review)
                .Without(x => x.ReviewPenaltys)
                .Without(x => x.VoteCategory);
        }

        private IEnumerable<VoteCategoryEntity> CreateVoteCategories(LeagueEntity league)
        {
            return fixture.Build<VoteCategoryEntity>()
                .With(x => x.League, league)
                .Without(x => x.AcceptedReviewVotes)
                .Without(x => x.CommentReviewVotes)
                .CreateMany();
        }
    }
}
