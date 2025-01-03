using Microsoft.Extensions.DependencyInjection;

namespace iRLeagueApiCore.Services.ResultService.Excecution;

internal sealed class StandingCalculationQueue : IStandingCalculationQueue
{
    private readonly IServiceProvider serviceProvider;
    private readonly IBackgroundTaskQueue taskQueue;

    public StandingCalculationQueue(IServiceProvider serviceProvider, IBackgroundTaskQueue taskQueue)
    {
        this.serviceProvider = serviceProvider;
        this.taskQueue = taskQueue;
    }

    public async Task QueueStandingCalculationAsync(long eventId)
    {
        var scope = serviceProvider.CreateScope();
        await taskQueue.QueueBackgroundWorkItemAsync(async cancellationToken =>
        {
            var standingCalculation = scope.ServiceProvider.GetRequiredService<ExecuteStandingCalculation>();
            await standingCalculation.Execute(eventId, cancellationToken);
            scope.Dispose();
        });
    }

    public void QueueStandingCalculationDebounced(long eventId, int debounceMs)
    {
        var scope = serviceProvider.CreateScope();
        var key = $"{nameof(ExecuteStandingCalculation)}_{eventId}";
        taskQueue.QueueBackgroundWorkItemDebounced(async cancellationToken =>
        {
            var standingCalculation = scope.ServiceProvider.GetRequiredService<ExecuteStandingCalculation>();
            await standingCalculation.Execute(eventId, cancellationToken);
            scope.Dispose();
        }, key, debounceMs);
    }
}
