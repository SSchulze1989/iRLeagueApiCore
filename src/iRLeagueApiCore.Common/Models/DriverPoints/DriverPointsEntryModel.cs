namespace iRLeagueApiCore.Common.Models.DriverPoints;

public class DriverPointsEntryModel
{
    public long AccountEntryId { get; set; }
    public long AccountId { get; set; }
    public DateTime CreatedOn { get; set; }
    public long CreatedById { get; set; }
    public string Description { get; set; } = string.Empty;
    public PenaltyModel Value { get; set; } = new PenaltyModel();
    public DateTime? ArchivedOn { get; set; }
    public string SourceRef { get; set; } = string.Empty;
    // EntryKind reference
    public long? EntryKindId { get; set; }
}
