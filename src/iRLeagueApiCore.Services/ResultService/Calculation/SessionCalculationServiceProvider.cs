using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class SessionCalculationServiceProvider :
        ICalculationServiceProvider<SessionCalculationConfiguration, SessionCalculationData, SessionCalculationResult>
    {
        public ICalculationService<SessionCalculationData, SessionCalculationResult> GetCalculationService(SessionCalculationConfiguration config)
        {
            return config.ResultKind switch
            {
                ResultKind.Member => new MemberSessionCalculationService(config),
                ResultKind.Team => throw new NotImplementedException("Team scoring is not implemented"),
                _ => throw new InvalidOperationException($"Unknown Scoring Kind: {config.ResultKind}"),
            };
        }
    }
}
