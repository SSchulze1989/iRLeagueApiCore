using iRLeagueApiCore.Common.Models; // PenaltyModel
using iRLeagueApiCore.Common.Enums; // EntryType

namespace iRLeagueApiCore.Common.Models.DriverPoints;

public class CreateDriverPointsEntryModel
{
    public long CreatedById { get; set; }
    public string Description { get; set; } = string.Empty;
    public PenaltyModel Value { get; set; } = new PenaltyModel();
    public string SourceRef { get; set; } = string.Empty;
    public long EntryKindId { get; set; }
}
