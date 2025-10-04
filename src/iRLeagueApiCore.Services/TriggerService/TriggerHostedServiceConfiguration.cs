namespace iRLeagueApiCore.Services.TriggerService;
public sealed class TriggerHostedServiceConfiguration
{
    public TimeSpan ScanTriggersInterval { get; set; } = TimeSpan.FromMinutes(1);
}
