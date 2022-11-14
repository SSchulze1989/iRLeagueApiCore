namespace iRLeagueApiCore.Services.ResultService.Excecution
{
    internal sealed class ResultCalculationQueue : IResultCalculationQueue
    {
        private readonly ExecuteEventResultCalculation resultCalculation;
        private readonly IBackgroundTaskQueue taskQueue;

        public ResultCalculationQueue(ExecuteEventResultCalculation resultCalculation, IBackgroundTaskQueue taskQueue)
        {
            this.resultCalculation = resultCalculation;
            this.taskQueue = taskQueue;
        }

        public async Task QueueEventResultAsync(long eventId)
        {
            await taskQueue.QueueBackgroundWorkItemAsync(cancellationToken => resultCalculation.Execute(eventId, cancellationToken));
        }
    }
}
