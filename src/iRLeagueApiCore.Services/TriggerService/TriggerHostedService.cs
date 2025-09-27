using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.TriggerService.Actions;
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
                    using var dbContext = scope.ServiceProvider.GetRequiredService<LeagueDbContext>();
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

    private async Task ExecuteTrigger(TriggerEntity trigger, CancellationToken cancellationToken)
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

    private async Task ScanTriggers(LeagueDbContext dbContext, CancellationToken cancellationToken)
    {
        // Placeholder for trigger scanning logic
        var now = DateTimeOffset.Now;
        _logger.LogInformation("Scanning triggers at: {time}", now);

        var timeTriggers = await dbContext.Triggers
            .IgnoreQueryFilters()
            .Where(x => x.TriggerType == TriggerType.Time)
            .Where(x => x.TimeElapesd != null && x.TimeElapesd <= now)
            .ToListAsync(cancellationToken);

        foreach (var trigger in timeTriggers)
        {
            await ExecuteTrigger(trigger, cancellationToken);
            if (trigger.Parameters.OnlyOnce)
            {
                trigger.TimeElapesd = null;
            }
            else if (!trigger.Parameters.OnlyOnce && trigger.Parameters.Interval is not null)
            {
                trigger.TimeElapesd += trigger.Parameters.Interval;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
