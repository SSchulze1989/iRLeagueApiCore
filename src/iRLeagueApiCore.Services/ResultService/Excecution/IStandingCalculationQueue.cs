namespace iRLeagueApiCore.Services.ResultService.Excecution;

public interface IStandingCalculationQueue
{
    public Task QueueStandingCalculationAsync(long eventId, bool skipNotifications = false);
    public void QueueStandingCalculationDebounced(long eventId, int debounceMs, bool skipNotifications = false);
}
