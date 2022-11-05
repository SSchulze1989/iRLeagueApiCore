using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation
{
    public sealed class EventResultCalculationServiceProviderTests
    {
        private readonly Fixture fixture;
        private readonly Mock<ICalculationServiceProvider<SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult>>
            mockServiceProvider;

        public EventResultCalculationServiceProviderTests()
        {
            fixture = new();
            mockServiceProvider = new Mock<ICalculationServiceProvider<SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult>>();
            mockServiceProvider.Setup(x => x.GetCalculationService(It.IsAny<SessionResultCalculationConfiguration>()))
                .Returns(() => Mock.Of<ICalculationService<SessionResultCalculationData, SessionResultCalculationResult>>());
            fixture.Register(() => mockServiceProvider.Object);
        }

        [Fact]
        public void GetCalculationService_ShouldProvideEventCalculationService()
        {
            var config = GetCalculationConfiguration();
            var sut = CreateSut();

            var test = sut.GetCalculationService(config);

            test.Should().BeOfType<EventResultCalculationService>();
        }

        private EventResultCalculationServiceProvider CreateSut()
        {
            return fixture.Create<EventResultCalculationServiceProvider>();
        }

        private EventResultCalculationConfiguration GetCalculationConfiguration()
        {
            return fixture.Create<EventResultCalculationConfiguration>();
        }
    }
}
