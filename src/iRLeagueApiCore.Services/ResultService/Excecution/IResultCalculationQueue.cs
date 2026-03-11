namespace iRLeagueApiCore.Services.ResultService.Excecution;

public interface IResultCalculationQueue
{
    public Task QueueEventResultAsync(long eventId, bool skipNotifications = false);
    public void QueueEventResultDebounced(long eventId, int debounceMs, bool skipNotifications = false);
}
