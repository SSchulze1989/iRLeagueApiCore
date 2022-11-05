namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class EventCalculationConfiguration
    {
        public long LeagueId { get; set; }
        public long EventId { get; set; }
        public long? ResultConfigId { get; set; }

        public string DisplayName { get; set; } = string.Empty;
        public IEnumerable<SessionCalculationConfiguration> SessionResultConfigurations { get; set; } = Array.Empty<SessionCalculationConfiguration>();
    }
}