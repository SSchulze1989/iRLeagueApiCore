namespace iRLeagueApiCore.Services.ResultService.Excecution;

public interface IStandingCalculationQueue
{
    public Task QueueStandingCalculationAsync(long eventId);
    public void QueueStandingCalculationDebounced(long eventId, int debounceMs);
}
