namespace iRLeagueApiCore.Services.TriggerService;
public sealed class TriggerHostedServiceConfiguration
{
    public int QueueCapacity { get; set; } = 100;
    public TimeSpan ScanTriggersInterval { get; set; } = TimeSpan.FromMinutes(1);
    public int MaxConcurrentTriggers { get; set; } = 5;
}
