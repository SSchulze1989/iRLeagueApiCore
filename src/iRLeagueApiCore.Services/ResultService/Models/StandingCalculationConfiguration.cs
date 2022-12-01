namespace iRLeagueApiCore.Services.ResultService.Models;

internal sealed class StandingCalculationConfiguration
{
    public long LeagueId { get; set; }
    public long SeasonId { get; set; }
    public long EventId { get; set; }
    public long? ResultConfigId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public int WeeksCounted { get; set; }
}
