using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.Calculation
{
    public sealed class SessionResultCalculationServiceProviderTests
    {
        private readonly Fixture fixture;

        public SessionResultCalculationServiceProviderTests()
        {
            fixture = new();
        }

        [Fact]
        public void GetCalculationService_ShouldProvideMemberCalculationService_WhenScoringKindIsMember()
        {
            var config = GetCalculationConfiguration();
            config.ScoringKind = Common.Enums.ScoringKind.Member;
            var sut = CreateSut();
            
            var test = sut.GetCalculationService(config);

            test.Should().BeOfType<MemberSessionResultCalculationService>();
        }

        [Fact]
        public void GetCalculationService_ShouldThrowNotImplementedException_WhenScoringKindIsCustom()
        {
            var config = GetCalculationConfiguration();
            config.ScoringKind = Common.Enums.ScoringKind.Custom;
            var sut = CreateSut();

            var test = () => sut.GetCalculationService(config);

            test.Should().Throw<NotImplementedException>();
        }

        private SessionResultCalculationServiceProvider CreateSut()
        {
            return fixture.Create<SessionResultCalculationServiceProvider>();
        }

        private SessionResultCalculationConfiguration GetCalculationConfiguration()
        {
            return fixture.Create<SessionResultCalculationConfiguration>();
        }
    }
}
