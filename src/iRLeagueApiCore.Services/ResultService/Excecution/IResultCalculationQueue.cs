namespace iRLeagueApiCore.Services.ResultService.Excecution;

public interface IResultCalculationQueue
{
    public Task QueueEventResultAsync(long eventId);
    public void QueueEventResultDebounced(long eventId, int debounceMs);
}
