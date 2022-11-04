using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class EventResultCalculationService : ICalculationService<EventResultCalculationData, EventResultCalculationResult>
    {
        private readonly EventResultCalculationConfiguration config;
        private readonly ICalculationServiceProvider< SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult> 
            sessionCalculationServiceProvider;

        public EventResultCalculationService(EventResultCalculationConfiguration config,
            ICalculationServiceProvider<SessionResultCalculationConfiguration, SessionResultCalculationData, SessionResultCalculationResult> sessionCalculationServiceProvider)
        {
            this.config = config;
            this.sessionCalculationServiceProvider = sessionCalculationServiceProvider;
        }

        public async Task<EventResultCalculationResult> Calculate(EventResultCalculationData data)
        {
            if (config.EventId != data.EventId)
            {
                throw new InvalidOperationException($"EventId in configuration and provided data set does not match -> config:{config.EventId} | data:{data.EventId}");
            }

            EventResultCalculationResult result = new();
            result.LeagueId = config.LeagueId;
            result.EventId = config.EventId;
            result.ResultConfigId = config.ResultConfigId;
            result.Name = config.DisplayName;
            List<SessionResultCalculationResult> sessionResults = new();
            foreach(var sessionConfig in config.SessionResultConfigurations)
            {
                var sessionData = data.SessionResults
                    .FirstOrDefault(x => x.SessionId == sessionConfig.SessionId);
                if (sessionData == null)
                {
                    continue;
                }
                var sessionCalculationService = sessionCalculationServiceProvider.GetCalculationService(sessionConfig);
                sessionResults.Add(await sessionCalculationService.Calculate(sessionData));
            }
            result.SessionResults = sessionResults;

            return result;
        }
    }
}
