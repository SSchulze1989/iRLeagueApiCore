using iRLeagueApiCore.Services.ResultService.Calculation.Filters;

namespace iRLeagueApiCore.Services.ResultService.Models;
internal sealed class BonusPointConfiguration
{
    public BonusPointType Type { get; set; }
    public int Value { get; set; }
    public int Points { get; set; }
    public FilterGroupRowFilter<ResultRowCalculationResult> Conditions { get; set; } = new();
    /// <summary>
    /// Maximum number of drivers/teams that are allowed to receive this bonus - if the conditions apply to more than this number the bonus will not be applied
    /// </summary>
    public int MaxCount { get; set; }
}
