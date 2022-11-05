namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class EventCalculationResult
    {
        public long LeagueId { get; set; }
        public long EventId { get; set; }
        public long? ResultConfigId { get; set; }
        public string Name { get; set; } = string.Empty;
        public IEnumerable<SessionCalculationResult> SessionResults { get; set; } = Array.Empty<SessionCalculationResult>();
    }
}
