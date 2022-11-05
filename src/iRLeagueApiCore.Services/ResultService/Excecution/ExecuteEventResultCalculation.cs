using iRLeagueApiCore.Services.ResultService.Calculation;
using iRLeagueApiCore.Services.ResultService.DataAccess;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Excecution
{
    internal sealed class ExecuteEventResultCalculation
    {
        private readonly ILogger<ExecuteEventResultCalculation> logger;
        private readonly IEventResultCalculationDataProvider dataProvider;
        private readonly IEventCalculationConfigurationProvider configProvider;
        private readonly IEventResultCalculationResultStore dataStore;
        private readonly ICalculationServiceProvider<EventResultCalculationConfiguration, EventResultCalculationData, EventResultCalculationResult> calculationServiceProvider;

        public ExecuteEventResultCalculation(ILogger<ExecuteEventResultCalculation> logger,
            IEventResultCalculationDataProvider dataProvider, 
            IEventCalculationConfigurationProvider configProvider, 
            IEventResultCalculationResultStore dataStore, 
            ICalculationServiceProvider<EventResultCalculationConfiguration, EventResultCalculationData, EventResultCalculationResult> calculationServiceProvider)
        {
            this.logger = logger;
            this.dataProvider = dataProvider;
            this.configProvider = configProvider;
            this.dataStore = dataStore;
            this.calculationServiceProvider = calculationServiceProvider;
        }

        public async ValueTask Execute(long eventId, CancellationToken cancellationToken = default)
        {
            using var loggerScoppe = logger.BeginScope(new Dictionary<string, object> { ["ExecutionId"] = new Guid() });

            logger.LogInformation("--- Start result calculation for event: {EventId} ---", eventId);
            IEnumerable<long?> resultConfigIds = (await configProvider.GetResultConfigIds(eventId, cancellationToken)).Cast<long?>();
            if (resultConfigIds.Any() == false)
            {
                resultConfigIds = new[] { default(long?) };
                logger.LogInformation("No result config found -> Using default.");
            }

            var eventResultCount = 0;
            logger.LogInformation("Calculating results for config ids: [{ResultConfigIds}]", resultConfigIds);
            try
            {
                foreach (var configId in resultConfigIds)
                {
                    try
                    {
                        var config = await configProvider.GetConfiguration(eventId, configId, cancellationToken);
                        var data = await dataProvider.GetData(config, cancellationToken);
                        if (data == null)
                        {
                            logger.LogInformation("No result data available for event: {EventId} | config: {ConfigId}", eventId, configId);
                            continue;
                        }
                        var calculationService = calculationServiceProvider.GetCalculationService(config);
                        var result = await calculationService.Calculate(data);
                        logger.LogInformation("Result calculated for event: {EventId} | config: {ConfigId}\n" +
                            " - SessionResults: {SessionResultCount}\n" +
                            " - ResultRows: {ResultRowCount}", eventId, configId, data.SessionResults.Count(), data.SessionResults.SelectMany(x => x.ResultRows).Count());
                        await dataStore.StoreCalculationResult(result, cancellationToken);
                        eventResultCount++;
                        logger.LogInformation("Results stored");
                    }
                    catch (Exception ex) when (ex is AggregateException || ex is InvalidOperationException || ex is NotImplementedException)
                    {
                        logger.LogError("Error thrown while calculating results for configId ({ConfigId}): {Exception}", configId, ex);
                    }
                }
                logger.LogInformation("Results calculated for event: {EventId}\n" +
                    " - EventResults: {EventResultCount}", eventId, eventResultCount);
                logger.LogInformation("--- Result calculation finished successfully ---");
            }
            catch (Exception ex) when (ex is AggregateException || ex is InvalidOperationException || ex is NotImplementedException)
            {
                logger.LogError("Error thrown while calculating results: {Exception}", ex);
            }
        }
    }
}
