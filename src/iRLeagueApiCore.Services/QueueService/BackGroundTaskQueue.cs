// from ASP.NET Core documentation:
// https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-6.0&tabs=visual-studio

using System.Threading.Channels;

public interface IBackgroundTaskQueue
{
    ValueTask QueueBackgroundWorkItemAsync(Func<CancellationToken, ValueTask> workItem);
    void QueueBackgroundWorkItemDebounced(Func<CancellationToken, ValueTask> workItem, object key, int debounceMs);

    ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken);
}

public sealed class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly ILogger<BackgroundTaskQueue> logger;

    private readonly Channel<Func<CancellationToken, ValueTask>> queue;

    private readonly Dictionary<object, System.Timers.Timer> debouncedTasks;

    public BackgroundTaskQueue(ILogger<BackgroundTaskQueue> logger, int capacity)
    {
        // Capacity should be set based on the expected application load and
        // number of concurrent threads accessing the queue.            
        // BoundedChannelFullMode.Wait will cause calls to WriteAsync() to return a task,
        // which completes only when space became available. This leads to backpressure,
        // in case too many publishers/calls start accumulating.
        this.logger = logger;
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        queue = Channel.CreateBounded<Func<CancellationToken, ValueTask>>(options);
        debouncedTasks = [];
    }

    public async ValueTask QueueBackgroundWorkItemAsync(
        Func<CancellationToken, ValueTask> workItem)
    {
        ArgumentNullException.ThrowIfNull(workItem);
        await queue.Writer.WriteAsync(workItem);
    }

    public void QueueBackgroundWorkItemDebounced(Func<CancellationToken, ValueTask> workItem, object key, int debounceMs)
    {
        ArgumentNullException.ThrowIfNull(nameof(workItem));
        if (debounceMs <= 0)
        {
            debounceMs = 1;
        }

        // Check if this task has already been queued
        var debouncedTimer = debouncedTasks.GetValueOrDefault(key);
        if (debouncedTimer is not null)
        {
            // reset debounce timer
            debouncedTimer.Stop();
            debouncedTimer.Interval = debounceMs;
            debouncedTimer.Start();
            logger.LogDebug("Reset debounce timer for background task '{BackgroundTaskId}' - wait for debounce {DebounceMs} ms.", key, debounceMs);
            return;
        }

        // Queue task with debounced timer
        var timer = new System.Timers.Timer()
        {
            AutoReset = false,
            Interval = debounceMs,
        };
        timer.Elapsed += async (sender, e) =>
        {
            debouncedTasks.Remove(key);
            await QueueBackgroundWorkItemAsync(workItem);
        };
        debouncedTasks.Add(key, timer);
        timer.Start();
        logger.LogDebug("Queued background task '{BackgroundTaskId}' - wait for debounce {DebounceMs} ms.", key, debounceMs);
    }

    public async ValueTask<Func<CancellationToken, ValueTask>> DequeueAsync(
        CancellationToken cancellationToken)
    {
        var workItem = await queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}