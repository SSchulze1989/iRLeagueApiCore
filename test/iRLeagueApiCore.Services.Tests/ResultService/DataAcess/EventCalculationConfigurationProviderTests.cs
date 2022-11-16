using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess
{
    [Collection("DataAccessTests")]
    public class EventCalculationConfigurationProviderTests : IAsyncLifetime
    {
        private readonly Fixture fixture;
        private readonly DataAccessMockHelper accessMockHelper;
        private readonly LeagueDbContext dbContext;

        public EventCalculationConfigurationProviderTests()
        {
            fixture = new Fixture();
            accessMockHelper = new();
            dbContext = accessMockHelper.CreateMockDbContext();
            fixture.Register(() => dbContext);
            var sessionConfigurationProvider = fixture.Create<SessionCalculationConfigurationProvider>();
            fixture.Register<ISessionCalculationConfigurationProvider>(() => sessionConfigurationProvider);
        }

        public async Task InitializeAsync()
        {
            await accessMockHelper.PopulateBasicTestSet(dbContext);
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
            test.Should().BeEquivalentTo(configs.Select(x => x.ResultConfigId));
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
        public async Task GetConfiguration_ShouldReturnConfiguration_WhenResultConfigIsNotNull()
        {
            var @event = await dbContext.Events
                .Include(x => x.Sessions)
                .FirstAsync();
            var config = accessMockHelper.CreateConfiguration(@event);
            dbContext.ResultConfigurations.Add(config);
            await dbContext.SaveChangesAsync();
            var sut = CreateSut();

            var test = await sut.GetConfiguration(@event.EventId, config.ResultConfigId);

            test.ResultConfigId = config.ResultConfigId;
        }

        [Fact]
        public async Task GetConfiguration_ShouldProvideDefaultResultId_WhenResultExistsAndConfigIsNull()
        {
            var @event = await dbContext.Events
                .Include(x => x.Sessions)
                .FirstAsync();
            var config = (ResultConfigurationEntity?)null;
            var eventResult = accessMockHelper.CreateScoredResult(@event, config);
            dbContext.ScoredEventResults.Add(eventResult);
            await dbContext.SaveChangesAsync();
            var sut = CreateSut();

            var test = await sut.GetConfiguration(@event.EventId, config?.ResultConfigId);

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

            var test = await sut.GetConfiguration(@event.EventId, config.ResultConfigId);

            test.ResultConfigId = config.ResultConfigId;
            test.ResultId.Should().Be(eventResult.ResultId);
        }

        private EventCalculationConfigurationProvider CreateSut()
        {
            return fixture.Create<EventCalculationConfigurationProvider>();
        }

        public async Task DisposeAsync()
        {
            await dbContext.DisposeAsync();
        }
    }
}
