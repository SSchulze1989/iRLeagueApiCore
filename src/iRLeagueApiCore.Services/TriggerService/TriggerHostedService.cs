using iRLeagueApiCore.Common.Enums;
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

    public TriggerHostedService(ILogger<TriggerHostedService> logger, TriggerHostedServiceConfiguration configuration, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _configuration = configuration;
        _serviceProvider = serviceProvider;
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
                    var dbContext = scope.ServiceProvider.GetRequiredService<ILeagueDbContext>();
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
    }

    private async Task ExecuteTrigger(TriggerEntity trigger, ILeagueDbContext dbContext, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Executing trigger {triggerId}: {triggerName}", trigger.TriggerId, trigger.Name);
        await Task.CompletedTask;
    }

    private async Task ScanTriggers(ILeagueDbContext dbContext, CancellationToken cancellationToken)
    {
        // Placeholder for trigger scanning logic
        _logger.LogInformation("Scanning triggers at: {time}", DateTimeOffset.Now);

        var timeTriggers = await dbContext.Triggers
            .IgnoreQueryFilters()
            .Where(x => x.TriggerType == TriggerType.Time)
            .Where(x => x.Parameters.TimeElapesd != null && x.Parameters.TimeElapesd <= DateTimeOffset.Now)
            .ToListAsync(cancellationToken);

        foreach (var trigger in timeTriggers)
        {
            await ExecuteTrigger(trigger, dbContext, cancellationToken);
        }

        await Task.CompletedTask;
    }
}
