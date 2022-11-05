using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Extensions;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation
{
    public sealed class EventResultCalculationServiceTests
    {
        private readonly Fixture fixture;
        private readonly Mock<ICalculationService<SessionResultCalculationData, SessionResultCalculationResult>> mockService;
        private readonly Mock<ICalculationServiceProvider<SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult>> 
            mockServiceProvider;

        public EventResultCalculationServiceTests()
        {
            fixture = new();
            mockService = new Mock<ICalculationService<SessionResultCalculationData, SessionResultCalculationResult>>();
            mockService.Setup(x => x.Calculate(It.IsAny<SessionResultCalculationData>()))
                .ReturnsAsync(() => fixture.Create<SessionResultCalculationResult>())
                .Verifiable();
            mockServiceProvider = new Mock<ICalculationServiceProvider< SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult>>();
            mockServiceProvider
                .Setup(x => x.GetCalculationService(It.IsAny<SessionResultCalculationConfiguration>()))
                .Returns(() => mockService.Object)
                .Verifiable();
            fixture.Register(() => mockServiceProvider.Object);
        }

        [Fact]
        public async Task Calculate_ShouldThrow_WhenEventIdDoesNotMatch()
        {
            var data = GetCalculationData();
            var config = GetCalculationConfiguration(data.LeagueId, fixture.Create<long>());
            var sut = CreateSut(config);

            var test = async () => await sut.Calculate(data);

            await test.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task Calculate_ShouldSetResultData()
        {
            var data = GetCalculationData();
            var config = GetCalculationConfiguration(data);
            var sut = CreateSut(config);

            var test = await sut.Calculate(data);

            test.LeagueId.Should().Be(config.LeagueId);
            test.EventId.Should().Be(config.EventId);
            test.ResultConfigId.Should().Be(config.ResultConfigId);
            test.Name.Should().Be(config.DisplayName);
            test.SessionResults.Should().HaveSameCount(config.SessionResultConfigurations);
        }

        [Fact]
        public async Task Calculate_ShouldCallCalculateForEachSession()
        {
            var data = GetCalculationData();
            var config = GetCalculationConfiguration(data);
            var sut = CreateSut(config);

            var test = await sut.Calculate(data);

            foreach(var session in data.SessionResults)
            {
                mockService.Verify(x => x.Calculate(session), Times.Once());
            }
        }

        [Fact]
        public async Task Calculate_ShouldNotCallCalculate_WhenNoMatchingConfigFound()
        {
            var data = GetCalculationData();
            var config = GetCalculationConfiguration(data);
            var addData = fixture.Create<SessionResultCalculationData>();
            data.SessionResults = data.SessionResults.Append(addData);
            var sut = CreateSut(config);

            var test = await sut.Calculate(data);

            test.SessionResults.Should().HaveCount(data.SessionResults.Count() - 1);
            mockService.Verify(x => x.Calculate(addData), Times.Never());
        }

        [Fact]
        public async Task Calculate_ShouldIngoreSessionConfig_WhenNoMatchingSessionFound()
        {
            var data = GetCalculationData();
            var config = GetCalculationConfiguration(data);
            var addConfig = fixture.Create<SessionResultCalculationConfiguration>();
            config.SessionResultConfigurations = config.SessionResultConfigurations.Append(addConfig);
            var sut = CreateSut(config);

            var test = await sut.Calculate(data);

            test.SessionResults.Should().HaveCount(config.SessionResultConfigurations.Count() - 1);
        }

        private EventResultCalculationService CreateSut(EventResultCalculationConfiguration config)
        {
            fixture.Register(() => config);
            return fixture.Create<EventResultCalculationService>();
        }

        private EventResultCalculationData GetCalculationData()
        {
            return fixture.Create<EventResultCalculationData>();
        }

        private EventResultCalculationConfiguration GetCalculationConfiguration(EventResultCalculationData data)
        {
            return GetCalculationConfiguration(data.LeagueId, data.EventId, data.SessionResults.Select(x => x.SessionId).NotNull());
        }

        private EventResultCalculationConfiguration GetCalculationConfiguration(long leagueId, long eventId, IEnumerable<long>? sessionIds = default)
        {
            sessionIds ??= Array.Empty<long>();
            var config = fixture.Build<EventResultCalculationConfiguration>()
                .With(x => x.LeagueId, leagueId)
                .With(x => x.EventId, eventId)
                .Create();
            foreach((var sessionConfig, var index) in config.SessionResultConfigurations.Select((x, i) => (x, i)))
            {
                sessionConfig.SessionId = sessionIds.ElementAtOrDefault(index);
            }
            return config;
        }
    }
}
