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
    private async Task QueueTriggerFunc(TriggerEntity trigger, TriggerParameterModel triggerParameters, CancellationToken cancellationToken)
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
                await triggerAction.ExecuteAsync(triggerParameters, trigger.ActionParameters, token);
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
            var parameters = new TriggerParameterModel
            {
                Interval = trigger.Interval,
                Time = trigger.TimeElapses,
                RefId1 = trigger.RefId1,
                RefId2 = trigger.RefId2,
            };
            await QueueTriggerFunc(trigger, parameters, cancellationToken);

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
                TriggerEventType.ResultDeleted => eventTriggersQuery.Where(x => x.RefId1 == null || x.RefId1 == parameters.RefId1),
                _ => ((List<TriggerEntity>)[]).AsQueryable(),
            };

            var eventTriggers = await eventTriggersQuery.ToListAsync(cancellationToken);

            foreach (var trigger in eventTriggers)
            {
                await QueueTriggerFunc(trigger, parameters, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during processing of event triggers of type {EventType} and parameters {Parameters}", eventType, parameters);
        }
    }

    public async Task ProcessManualTrigger(LeagueDbContext dbContext, long triggerId, TriggerParameterModel parameters, CancellationToken cancellationToken)
    {
        try
        {
            var trigger = await dbContext.Triggers
                .Where(x => !x.IsArchived)
                .Where(x => x.TriggerId == triggerId)
                .FirstOrDefaultAsync(cancellationToken);
            if (trigger == null)
            {
                _logger.LogWarning("Manual trigger with id {TriggerId} not found or is archived.", triggerId);
                return;
            }
            if (trigger.TriggerType != TriggerType.Manual)
            {
                _logger.LogWarning("Trigger with id {TriggerId} is not a manual trigger.", triggerId);
                return;
            }
            await QueueTriggerFunc(trigger, parameters, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during processing of manual trigger with id {TriggerId}", triggerId);
        }
    }
}
