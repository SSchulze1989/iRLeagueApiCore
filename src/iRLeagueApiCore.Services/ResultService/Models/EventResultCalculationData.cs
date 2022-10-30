namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class EventResultCalculationData
    {
        public long LeagueId { get; set; }
        public long EventId { get; set; }
        public IEnumerable<SessionResultCalculationData> SessionResults { get; set; } = Array.Empty<SessionResultCalculationData>();
    }
}
