using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Excecution;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.Tests.Extensions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;

namespace iRLeagueApiCore.Services.Tests.ResultService.Execution
{
    public sealed class ExecuteEventResultCalculationTests
    {
        private readonly Fixture fixture;
        private readonly ILogger<ExecuteEventResultCalculation> logger;
        private readonly Mock<IEventCalculationConfigurationProvider> mockConfigurationProvider;
        private readonly Mock<IEventResultCalculationDataProvider> mockDataProvider;
        private readonly Mock<IEventResultCalculationResultStore> mockResultStore;
        private readonly Mock<ICalculationServiceProvider<EventResultCalculationConfiguration,
            EventResultCalculationData, EventResultCalculationResult>> mockCalculationServiceProvider;

        public ExecuteEventResultCalculationTests(ITestOutputHelper testOutputHelper)
        {
            fixture = new();
            var mockLogger = new Mock<ILogger<ExecuteEventResultCalculation>>();
            mockLogger.Setup(x => x.BeginScope<It.IsAnyType>(It.IsAny<It.IsAnyType>()))
                .Returns(() => Mock.Of<IDisposable>());
            mockLogger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                    var eventId = (EventId)invocation.Arguments[1];  // so I'm not sure you would ever want to actually use them
                    var state = invocation.Arguments[2];
                    var exception = (Exception?)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = (string?)invokeMethod?.Invoke(formatter, new[] { state, exception });

                    testOutputHelper.WriteLine("{0}: {1}", logLevel, logMessage);
                }));
            logger = mockLogger.Object;
            mockConfigurationProvider = MockConfigurationProvider(fixture);
            mockDataProvider = MockDataProvider(fixture);
            mockResultStore = MockResultStore();
            mockCalculationServiceProvider = MockCalculationServiceProvider(fixture);
            fixture.Register(() => logger);
            fixture.Register(() => mockConfigurationProvider.Object);
            fixture.Register(() => mockDataProvider.Object);
            fixture.Register(() => mockResultStore.Object);
            fixture.Register(() => mockCalculationServiceProvider.Object);
        }

        [Fact]
        public async Task Execute_ShouldUseDefaultResultConfig_WhenProviderReturnsEmpty()
        {
            long eventId = fixture.Create<long>();
            mockConfigurationProvider.Setup(x => x.GetResultConfigIds(It.Is<long>(x => x == eventId), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => Array.Empty<long>());
            mockConfigurationProvider.Setup(x => x.GetConfiguration(It.Is<long>(x => x == eventId), It.Is<long?>(x => x == null), It.IsAny<CancellationToken>()))
                .ReturnsAsync((long eventId, long? resultConfigId, CancellationToken cancellationToken) => CreateConfiguration(fixture, eventId, resultConfigId))
                .Verifiable();
            var sut = CreateSut();

            await sut.Execute(eventId);

            mockConfigurationProvider
                .Verify(x => x.GetConfiguration(It.Is<long>(x => x == eventId), It.Is<long?>(x => x == null), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Execute_ShouldSkipResult_WhenDataProviderReturnsNull()
        {
            long eventId = fixture.Create<long>();
            mockDataProvider.Setup(x => x.GetData(It.IsAny<EventResultCalculationConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => null)
                .Verifiable();
            var mockCalculationService = new Mock<ICalculationService<EventResultCalculationData, EventResultCalculationResult>>();
            mockCalculationService.Setup(x => x.Calculate(It.IsAny<EventResultCalculationData>()))
                .ReturnsAsync(() => fixture.Create<EventResultCalculationResult>())
                .Verifiable();
            mockCalculationServiceProvider.Setup(x => x.GetCalculationService(It.IsAny<EventResultCalculationConfiguration>()))
                .Returns(mockCalculationService.Object);
            var sut = CreateSut();

            await sut.Execute(eventId);

            mockDataProvider.Verify(x => x.GetData(It.IsAny<EventResultCalculationConfiguration>(), It.IsAny<CancellationToken>()));
            mockCalculationService.Verify(x => x.Calculate(It.IsAny<EventResultCalculationData>()), Times.Never);
        }

        [Fact]
        public async Task Execute_ShouldCalculateForEachResultConfig()
        {
            long eventId = fixture.Create<long>();
            int resultConfigCount = 3;
            var resultConfigIds = fixture.CreateMany<long>(resultConfigCount);
            mockConfigurationProvider.Setup(x => x.GetResultConfigIds(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => resultConfigIds);
            var storedResults = new List<EventResultCalculationResult>();
            mockResultStore.Setup(x => x.StoreCalculationResult(It.IsAny<EventResultCalculationResult>(), It.IsAny<CancellationToken>()))
                .Callback((EventResultCalculationResult result, CancellationToken cancellationToken) => storedResults.Add(result));
            var sut = CreateSut();

            await sut.Execute(eventId);

            storedResults.Should().HaveCount(resultConfigCount);
            foreach((var result, var configId) in storedResults.Zip(resultConfigIds))
            {
                result.ResultConfigId.Should().Be(configId);
            }
        }

        private ExecuteEventResultCalculation CreateSut()
        {
            return fixture.Create<ExecuteEventResultCalculation>();
        }

        private static Mock<IEventCalculationConfigurationProvider> MockConfigurationProvider(Fixture fixture)
        {
            var mockProvider = new Mock<IEventCalculationConfigurationProvider>();
            mockProvider.Setup(x => x.GetResultConfigIds(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(() => fixture.CreateMany<long>())
                .Verifiable();
            mockProvider.Setup(x => x.GetConfiguration(It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((long eventId, long? resultConfigId, CancellationToken cancellationToken) => CreateConfiguration(fixture, eventId, resultConfigId))
                .Verifiable();
            return mockProvider;
        }

        private static EventResultCalculationConfiguration CreateConfiguration(Fixture fixture, long eventId, long? resultConfigurationId)
        {
            return fixture.Build<EventResultCalculationConfiguration>()
                .With(x => x.EventId, eventId)
                .With(x => x.ResultConfigId, resultConfigurationId)
                .Create();
        }

        private static Mock<IEventResultCalculationDataProvider> MockDataProvider(Fixture fixture)
        {
            var mockProvider = new Mock<IEventResultCalculationDataProvider>();
            mockProvider.Setup(x => x.GetData(It.IsAny<EventResultCalculationConfiguration>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((EventResultCalculationConfiguration config, CancellationToken cancellationToken) => CreateData(fixture, config))
                .Verifiable();

            return mockProvider;
        }

        private static EventResultCalculationData CreateData(Fixture fixture, EventResultCalculationConfiguration config)
        {
            var data = fixture.Build<EventResultCalculationData>()
                .With(x => x.EventId, config.EventId)
                .Without(x => x.SessionResults)
                .Create();
            data.SessionResults = fixture.Build<SessionResultCalculationData>()
                .WithSequence(x => x.SessionId, config.SessionResultConfigurations.Select(x => x.SessionId))
                .CreateMany(config.SessionResultConfigurations.Count());
            return data;
        }

        private static Mock<IEventResultCalculationResultStore> MockResultStore()
        {
            var mockStore = new Mock<IEventResultCalculationResultStore>();
            mockStore.Setup(x => x.StoreCalculationResult(It.IsAny<EventResultCalculationResult>(), It.IsAny<CancellationToken>()))
                .Verifiable();

            return mockStore;
        }

        private static Mock<ICalculationServiceProvider<EventResultCalculationConfiguration, EventResultCalculationData, EventResultCalculationResult>> MockCalculationServiceProvider(Fixture fixture)
        {
            var mockServiceProvider = new Mock<ICalculationServiceProvider<EventResultCalculationConfiguration, EventResultCalculationData, EventResultCalculationResult>>();
            mockServiceProvider.Setup(x => x.GetCalculationService(It.IsAny<EventResultCalculationConfiguration>()))
                .Returns((EventResultCalculationConfiguration config) => MockCalculationService(fixture, config).Object)
                .Verifiable();

            return mockServiceProvider;
        }

        private static Mock<ICalculationService<EventResultCalculationData, EventResultCalculationResult>> MockCalculationService(Fixture fixture, EventResultCalculationConfiguration config)
        {
            var mockService = new Mock<ICalculationService<EventResultCalculationData, EventResultCalculationResult>>();
            mockService.Setup(x => x.Calculate(It.IsAny<EventResultCalculationData>()))
                .ReturnsAsync((EventResultCalculationData data) => CreateEventCalculationResult(fixture, config, data))
                .Verifiable();

            return mockService;
        }

        private static EventResultCalculationResult CreateEventCalculationResult(Fixture fixture, EventResultCalculationConfiguration config, EventResultCalculationData data)
        {
            return new EventResultCalculationResult()
            {
                EventId = data.EventId,
                LeagueId = data.LeagueId,
                ResultConfigId = config.ResultConfigId,
                Name = config.DisplayName,
                SessionResults = data.SessionResults.Select(x => new SessionResultCalculationResult(x)),
            };
        }
    }
}
