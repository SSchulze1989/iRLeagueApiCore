using Microsoft.Extensions.Hosting;

namespace iRLeagueApiCore.Services.TriggerService;
public sealed class TriggerHostedService : BackgroundService
{
    private readonly ILogger<TriggerHostedService> _logger;
    private readonly TriggerHostedServiceConfiguration _configuration;

    public TriggerHostedService(ILogger<TriggerHostedService> logger, TriggerHostedServiceConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
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
                await ScanTriggers();
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

    private async Task ScanTriggers()
    {
        // Placeholder for trigger scanning logic
        _logger.LogInformation("Scanning triggers at: {time}", DateTimeOffset.Now);
        await Task.CompletedTask;
    }
}
