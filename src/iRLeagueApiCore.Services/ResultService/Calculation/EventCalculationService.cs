using iRLeagueApiCore.Common.Enums;
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
            foreach(var sessionConfig in config.SessionResultConfigurations.Where(x => x.IsCombinedResult == false).OrderBy(x => x.SessionNr))
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
            if (config.SessionResultConfigurations.Any(x => x.IsCombinedResult))
            {
                var combinedConfig = config.SessionResultConfigurations.First(x => x.IsCombinedResult);
                IEnumerable<ResultRowCalculationData> combinedRows;
                if (data.SessionResults.Any(x => x.SessionNr == 999) && combinedConfig.UseExternalSourcePoints)
                {
                    combinedRows = data.SessionResults.First(x => x.SessionNr == 999).ResultRows;
                }
                else
                {
                    var combinedSessionNrs = config.SessionResultConfigurations
                    .Where(x => x.IsCombinedResult == false)
                    .Where(x => x.SessionType == SessionType.Race)
                    .Select(x => x.SessionNr);
                    combinedRows = sessionResults
                        .Where(x => combinedSessionNrs.Contains(x.SessionNr))
                        .SelectMany(x => x.ResultRows);
                }
                if (combinedRows.Any())
                {
                    var combinedData = new SessionCalculationData()
                    {
                        LeagueId = combinedConfig.LeagueId,
                        SessionId = null,
                        SessionNr = 999,
                        ResultRows = combinedRows,
                    };
                    var combinedCalculationService = sessionCalculationServiceProvider.GetCalculationService(combinedConfig);
                    sessionResults.Add(await combinedCalculationService.Calculate(combinedData));
                }
            }
            result.SessionResults = sessionResults;

            return result;
        }
    }
}
