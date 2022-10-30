namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class EventResultCalculationResult
    {
        public long LeagueId { get; set; }
        public long EventId { get; set; }
        public long? ResultConfigId { get; set; }
        public string Name { get; set; } = string.Empty;
        public IEnumerable<SessionResultCalculationResult> SessionResults { get; set; } = Array.Empty<SessionResultCalculationResult>();
    }
}
