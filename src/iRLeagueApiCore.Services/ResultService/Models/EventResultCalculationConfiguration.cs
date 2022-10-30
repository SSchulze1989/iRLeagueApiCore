namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class EventResultCalculationConfiguration
    {
        public long LeagueId { get; set; }
        public long EventId { get; set; }
        public long? ResultConfigId { get; set; }

        public string DisplayName { get; set; } = string.Empty;
        public IEnumerable<SessionResultCalculationConfiguration> SessionResultConfigurations { get; set; } = Array.Empty<SessionResultCalculationConfiguration>();
    }
}