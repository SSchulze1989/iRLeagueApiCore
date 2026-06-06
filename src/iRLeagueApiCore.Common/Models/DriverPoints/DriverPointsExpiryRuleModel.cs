namespace iRLeagueApiCore.Common.Models.DriverPoints;

public class DriverPointsExpiryRuleModel
{
    public long ExpiryRuleId { get; set; }
    public PenaltyType? PenaltyType { get; set; }
    public int? IntervalDays { get; set; }
    public int? EventsDriven { get; set; }
    public int? EventsMissed { get; set; }
    public bool ResetOnNewSeason { get; set; }
    public string Description { get; set; }
}