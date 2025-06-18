
using System.Reflection.Emit;

namespace iRLeagueApiCore.Services.ResultService.Models;
internal class RosterEntryData
{
    public long MemberId { get; set; }
    public long TeamId { get; set; }
    public string Number { get; set; } = string.Empty;
}
