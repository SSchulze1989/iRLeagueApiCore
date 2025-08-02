using iRLeagueApiCore.Mocking.DataAccess;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAccess;

public sealed class EventCalculationConfigurationProviderTests : DataAccessTestsBase
{
    public EventCalculationConfigurationProviderTests()
    {
        var sessionConfigurationProvider = fixture.Create<SessionCalculationConfigurationProvider>();
        fixture.Register<ISessionCalculationConfigurationProvider>(() => sessionConfigurationProvider);
    }

    [Fact]
    public async Task GetResultConfigIds_ShouldReturnEmpty_WhenNoResulConfigConfigured()
    {
        var @event = await dbContext.Events
            .FirstAsync();
        var sut = CreateSut();

        var test = await sut.GetResultConfigIds(@event.EventId);

        test.Should().BeEmpty();
    }

    [Fact]
    public async Task GetResultConfigIds_ShouldReturnCollection_WhenResultConfigsConfigured()
    {
        var @event = await dbContext.Events
            .FirstAsync();
        var configs = accessMockHelper.ConfigurationBuilder(@event).CreateMany();
        dbContext.ResultConfigurations.AddRange(configs);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetResultConfigIds(@event.EventId);

        test.Should().HaveSameCount(configs);
        test.Should().BeEquivalentTo(configs.Select(x => x.PointSystemId));
    }

    [Fact]
    public async Task GetResultConfigIds_ShouldReturnInOrderOfDependency_WhenSourceResultConfigIsConfigured()
    {
        var @event = await dbContext.Events.FirstAsync();
        int resultConfigCount = 5;
        var resultConfigs = accessMockHelper.ConfigurationBuilder(@event).CreateMany(resultConfigCount).ToList();
        // add dependencies
        resultConfigs[0].SourcePointSystemId = resultConfigs[2].PointSystemId;
        resultConfigs[1].SourcePointSystemId = resultConfigs[0].PointSystemId;
        resultConfigs[2].SourcePointSystemId = resultConfigs[4].PointSystemId;
        resultConfigs[3].SourcePointSystemId = null;
        resultConfigs[4].SourcePointSystemId = resultConfigs[3].PointSystemId;
        dbContext.ResultConfigurations.AddRange(resultConfigs);
        await dbContext.SaveChangesAsync();
        var expectedOrder = new[] { 3, 4, 2, 0, 1 };
        var sut = CreateSut();

        var test = await sut.GetResultConfigIds(@event.EventId);

        foreach ((var configId, var expIndex) in test.Zip(expectedOrder))
        {
            configId.Should().Be(resultConfigs[expIndex].PointSystemId);
        }
    }

    [Fact]
    public async Task GetResultConfigIds_ShouldThrow_WhenSourceResultConfigContainsCyclicDependency()
    {
        var @event = await dbContext.Events.FirstAsync();
        int resultConfigCount = 5;
        var resultConfigs = accessMockHelper.ConfigurationBuilder(@event).CreateMany(resultConfigCount).ToList();
        // add dependencies
        resultConfigs[0].SourcePointSystemId = resultConfigs[1].PointSystemId;
        resultConfigs[1].SourcePointSystemId = resultConfigs[0].PointSystemId;
        resultConfigs[2].SourcePointSystemId = resultConfigs[4].PointSystemId;
        resultConfigs[3].SourcePointSystemId = null;
        resultConfigs[4].SourcePointSystemId = resultConfigs[3].PointSystemId;
        dbContext.ResultConfigurations.AddRange(resultConfigs);
        await dbContext.SaveChangesAsync();
        var expectedOrder = new[] { 3, 4, 2, 0, 1 };
        var sut = CreateSut();

        var test = async () => await sut.GetResultConfigIds(@event.EventId);

        await test.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetConfiguration_ShouldReturnDefaultConfiguration_WhenResultConfigIsNull()
    {
        var @event = await dbContext.Events
            .Include(x => x.Sessions)
            .FirstAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(@event.EventId, null);

        test.LeagueId.Should().Be(@event.LeagueId);
        test.EventId.Should().Be(@event.EventId);
        test.DisplayName.Should().Be("Default");
        test.ResultConfigId.Should().BeNull();
        test.SessionResultConfigurations.Should().HaveSameCount(@event.Sessions);
    }

    [Fact]
    public async Task GetConfiguration_ShouldReturnConfiguration_WithChampSeasonId()
    {
        var @event = await dbContext.Events
            .Include(x => x.Schedule)
                .ThenInclude(x => x.Season)
            .Include(x => x.Sessions)
            .FirstAsync();
        var championship = await dbContext.Championships.FirstAsync();
        var champSeason = accessMockHelper.CreateChampSeason(championship, @event.Schedule.Season);
        var config = accessMockHelper.CreateConfiguration(@event);
        champSeason.PointSystems = new[] { config };
        dbContext.ChampSeasons.Add(champSeason);
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(@event.EventId, config.PointSystemId);

        test.ChampSeasonId.Should().Be(champSeason.ChampSeasonId);
    }

    [Fact]
    public async Task GetConfiguration_ShouldReturnConfiguration_WhenResultConfigIsNotNull()
    {
        var @event = await dbContext.Events
            .Include(x => x.Sessions)
            .FirstAsync();
        var config = accessMockHelper.CreateConfiguration(@event);
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(@event.EventId, config.PointSystemId);

        test.ResultConfigId.Should().Be(config.PointSystemId);
    }

    [Fact]
    public async Task GetConfiguration_ShouldProvideDefaultResultId_WhenResultExistsAndConfigIsNull()
    {
        var @event = await dbContext.Events
            .Include(x => x.Sessions)
            .FirstAsync();
        var config = (PointSystemEntity?)null;
        var eventResult = accessMockHelper.CreateScoredResult(@event, config);
        dbContext.ScoredEventResults.Add(eventResult);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(@event.EventId, config?.PointSystemId);

        test.ResultId.Should().Be(eventResult.ResultId);
    }

    [Fact]
    public async Task GetConfiguration_ShouldProvideConfigurationResultId_WhenResultExistsAndConfigIsNotNull()
    {
        var @event = await dbContext.Events
            .Include(x => x.Sessions)
            .FirstAsync();
        var config = accessMockHelper.CreateConfiguration(@event);
        var eventResult = accessMockHelper.CreateScoredResult(@event, config);
        dbContext.ScoredEventResults.Add(eventResult);
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(@event.EventId, config.PointSystemId);

        test.ResultConfigId.Should().Be(config.PointSystemId);
        test.ResultId.Should().Be(eventResult.ResultId);
    }

    [Fact]
    public async Task GetConfiguration_ShouldProvideSourceConfigId_WhenSourceConfigIsConfigured()
    {
        var @event = await dbContext.Events
            .Include(x => x.Sessions)
            .FirstAsync();
        var sourceConfig = accessMockHelper.CreateConfiguration(@event);
        var config = accessMockHelper.CreateConfiguration(@event);
        config.SourcePointSystem = sourceConfig;
        dbContext.ResultConfigurations.Add(sourceConfig);
        dbContext.ResultConfigurations.Add(config);
        await dbContext.SaveChangesAsync();
        var sut = CreateSut();

        var test = await sut.GetConfiguration(@event.EventId, config.PointSystemId);

        test.ResultConfigId.Should().Be(config.PointSystemId);
        test.SourceResultConfigId.Should().Be(sourceConfig.PointSystemId);
    }

    private EventCalculationConfigurationProvider CreateSut()
    {
        return fixture.Create<EventCalculationConfigurationProvider>();
    }
}
