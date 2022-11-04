using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class EventResultCalculationServiceProvider : 
        ICalculationServiceProvider<EventResultCalculationConfiguration,EventResultCalculationData,EventResultCalculationResult>
    {
        private readonly ICalculationServiceProvider<SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult> 
            sessionCalculationServiceProvider;

        public EventResultCalculationServiceProvider(
            ICalculationServiceProvider<SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult> sessionCalculationServiceProvider)
        {
            this.sessionCalculationServiceProvider = sessionCalculationServiceProvider;
        }

        public ICalculationService<EventResultCalculationData, EventResultCalculationResult> GetCalculationService(EventResultCalculationConfiguration config)
        {
            return new EventResultCalculationService(config, sessionCalculationServiceProvider);
        }
    }
}
