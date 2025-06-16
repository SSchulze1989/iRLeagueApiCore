namespace iRLeagueApiCore.Services.ResultService.Models;
internal sealed class RosterData
{
    public long RosterId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public IEnumerable<RosterEntryData> Entries { get; set; } = [];
}
