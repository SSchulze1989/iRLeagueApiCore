﻿using iRLeagueApiCore.Services.ResultService.Extensions;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess;

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

        foreach (var @event in events)
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

    [Fact]
    public async Task PopulateBasicTestSet_ResultShouldHaveSessionResults()
    {
        using var dbContext = accessMockHelper.CreateMockDbContext();
        await accessMockHelper.PopulateBasicTestSet(dbContext);

        var result = await dbContext.EventResults
            .Include(x => x.SessionResults)
            .FirstAsync();

        result.SessionResults.Should().NotBeEmpty();
    }

    [Fact]
    public async Task PopulateBasicTestSet_SessionsShouldHaveAtLeastOneReview()
    {
        using var dbContext = accessMockHelper.CreateMockDbContext();
        await accessMockHelper.PopulateBasicTestSet(dbContext);

        var sessions = await dbContext.Sessions
            .Include(x => x.IncidentReviews)
            .ToListAsync();

        foreach (var session in sessions)
        {
            session.IncidentReviews.Should().NotBeEmpty();
        }
    }

    [Fact]
    public async Task PopulateBasicTestSet_ShouldHaveVoteCategories()
    {
        using var dbContext = accessMockHelper.CreateMockDbContext();
        await accessMockHelper.PopulateBasicTestSet(dbContext);

        var voteCategories = await dbContext.VoteCategories
            .ToListAsync();

        voteCategories.Should().NotBeEmpty();
    }

    [Fact]
    public async Task ShouldAddPointRule_WithoutDeletingScoring()
    {
        long configId;
        long pointRuleId;
        int scoringCount;

        using (var dbContext = accessMockHelper.CreateMockDbContext())
        {
            await accessMockHelper.PopulateBasicTestSet(dbContext);

            var @event = await dbContext.Events
                .Include(x => x.Schedule.Season.League)
                .FirstAsync();
            var config = accessMockHelper.CreateConfiguration(@event);
            var pointRule = accessMockHelper.CreatePointRule(@event.Schedule.Season.League);
            configId = config.ResultConfigId;
            pointRuleId = pointRule.PointRuleId;
            scoringCount = config.Scorings.Count();
            dbContext.ResultConfigurations.Add(config);
            dbContext.PointRules.Add(pointRule);
            config.Scorings.ForEeach(x => x.PointsRule = pointRule);
            await dbContext.SaveChangesAsync();
        }

        using (var dbContext = accessMockHelper.CreateMockDbContext())
        {
            var config = await dbContext.ResultConfigurations
                .Include(x => x.Scorings)
                .FirstAsync(x => x.ResultConfigId == configId);

            config.Scorings.Should().HaveCount(scoringCount);
        }
    }
}
