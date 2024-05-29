using iRLeagueApiCore.Common.Models;
using System.Runtime.Serialization;

namespace iRLeagueApiCore.Services.ResultService.Models;
internal sealed class DropweekOverrideData
{
    public long StandingConfigId { get; set; }
    public long ScoredResultRowId { get; set; }
    public bool ShouldDrop { get; set; }
    public string Reason { get; set; } = string.Empty;
}
