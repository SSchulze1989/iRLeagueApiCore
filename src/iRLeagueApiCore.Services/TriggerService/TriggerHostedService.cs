using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Services.TriggerService.Actions;
using iRLeagueApiCore.Services.TriggerService.Events;
using iRLeagueDatabaseCore;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace iRLeagueApiCore.Services.TriggerService;
public sealed class TriggerHostedService : BackgroundService
{
    private readonly ILogger<TriggerHostedService> _logger;
    private readonly TriggerHostedServiceConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly IBackgroundTaskQueue _taskQueue;

    public TriggerHostedService(ILogger<TriggerHostedService> logger, TriggerHostedServiceConfiguration configuration, IServiceProvider serviceProvider, IBackgroundTaskQueue taskQueue)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _taskQueue = taskQueue;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Trigger Hosted Service is running.");

        await TriggerServiceLoop(stoppingToken);
    }

    private async Task TriggerServiceLoop(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();
                    await ScanTriggers(dbContext, stoppingToken);
                }

                await Task.Delay(_configuration.ScanTriggersInterval, stoppingToken);
            }
            catch (TaskCanceledException)
            {
                // stopping token was triggered, exit the loop
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during trigger scan.");
            }
        }
        _logger.LogInformation("Trigger Hosted Service is stopped.");
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Trigger Hosted Service is stopping.");

        return base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Queues the execution of the specified trigger on the background task queue.
    /// </summary>
    /// <param name="trigger"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task QueueTriggerFunc(TriggerEntity trigger, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing trigger {triggerId}: {triggerName}", trigger.TriggerId, trigger.Name);
        await _taskQueue.QueueBackgroundWorkItemAsync(async token =>
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var leagueId = trigger.LeagueId;
                var leagueProvider = scope.ServiceProvider.GetRequiredService<ILeagueProvider>();
                leagueProvider.SetLeague(leagueId);
                var actionProvider = scope.ServiceProvider.GetRequiredService<TriggerActionProvider>();
                var triggerAction = actionProvider.GetTriggerAction(trigger.Action);
                await triggerAction.ExecuteAsync(trigger.ActionParameters, token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during execution of trigger {triggerId}: {triggerName}", trigger.TriggerId, trigger.Name);
            }
        });
        await Task.CompletedTask;
    }

    /// <summary>
    /// Scan for triggers that need to be executed and queues their execution. Only time-based triggers are scanned here.
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    private async Task ScanTriggers(LeagueDbContext dbContext, CancellationToken cancellationToken)
    {
        // Placeholder for trigger scanning logic
        var now = DateTimeOffset.Now;
        _logger.LogDebug("Scanning triggers at: {time}", now);

        var timeTriggers = await dbContext.Triggers
            .IgnoreQueryFilters()
            .Where(x => !x.IsArchived)
            .Where(x => x.TriggerType == TriggerType.Time || x.TriggerType == TriggerType.Interval)
            .Where(x => x.TimeElapses != null && x.TimeElapses <= now)
            .ToListAsync(cancellationToken);

        foreach (var trigger in timeTriggers)
        {
            await QueueTriggerFunc(trigger, cancellationToken);

            if (trigger.TriggerType == TriggerType.Interval)
            {
                trigger.TimeElapses += trigger.Interval.GetValueOrDefault();
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task ProcessEventTriggers(LeagueDbContext dbContext, TriggerEventType eventType, TriggerParameterModel parameters, CancellationToken cancellationToken)
    {
        try
        {
            var eventTriggersQuery = dbContext.Triggers
                .Where(x => !x.IsArchived)
                .Where(x => x.TriggerType == TriggerType.Event)
                .Where(x => x.EventType == eventType)
                .AsNoTracking();

            eventTriggersQuery = eventType switch
            {
                TriggerEventType.ResultUploaded or
                TriggerEventType.ResultCalculated or
                TriggerEventType.ResultUpdated or 
                TriggerEventType.ResultDeleted => eventTriggersQuery.Where(x => x.RefId1 == null || x.RefId1 == parameters.EventId),
                _ => ((List<TriggerEntity>)[]).AsQueryable(),
            };

            var eventTriggers = await eventTriggersQuery.ToListAsync(cancellationToken);

            foreach (var trigger in eventTriggers)
            {
                await QueueTriggerFunc(trigger, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during processing of event triggers of type {EventType} and parameters {Parameters}", eventType, parameters);
        }
    }
}
