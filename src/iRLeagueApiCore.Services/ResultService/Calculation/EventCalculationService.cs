using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class EventCalculationService : ICalculationService<EventCalculationData, EventCalculationResult>
    {
        private readonly EventCalculationConfiguration config;
        private readonly ICalculationServiceProvider< SessionCalculationConfiguration, SessionCalculationData, SessionCalculationResult> 
            sessionCalculationServiceProvider;

        public EventCalculationService(EventCalculationConfiguration config,
            ICalculationServiceProvider<SessionCalculationConfiguration, SessionCalculationData, SessionCalculationResult> sessionCalculationServiceProvider)
        {
            this.config = config;
            this.sessionCalculationServiceProvider = sessionCalculationServiceProvider;
        }

        public async Task<EventCalculationResult> Calculate(EventCalculationData data)
        {
            if (config.EventId != data.EventId)
            {
                throw new InvalidOperationException($"EventId in configuration and provided data set does not match -> config:{config.EventId} | data:{data.EventId}");
            }

            EventCalculationResult result = new(data);
            result.ResultId = config.ResultId;
            result.ResultConfigId = config.ResultConfigId;
            result.Name = config.DisplayName;
            List<SessionCalculationResult> sessionResults = new();
            foreach(var sessionConfig in config.SessionResultConfigurations)
            {
                var sessionData = data.SessionResults
                    .FirstOrDefault(x => x.SessionNr == sessionConfig.SessionNr);
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
