using iRLeagueApiCore.Common.Enums;

namespace iRLeagueApiCore.Services.ResultService.Models;

public sealed class AddPenaltyCalculationData
{
    public PenaltyType Type { get; set; }
    public double Points { get; set; }
    public int Positions { get; set; }
    public TimeSpan Time { get; set; }
}
