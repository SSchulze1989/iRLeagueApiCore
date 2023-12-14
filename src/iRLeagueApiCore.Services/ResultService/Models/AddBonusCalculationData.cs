namespace iRLeagueApiCore.Services.ResultService.Models;
internal sealed class AddBonusCalculationData
{
    public int SessionNr { get; set; }
    public long? MemberId { get; set; }
    public long? TeamId { get; set; }
    public string Reason { get; set; } = string.Empty;
    public double BonusPoints { get; set; }
}
