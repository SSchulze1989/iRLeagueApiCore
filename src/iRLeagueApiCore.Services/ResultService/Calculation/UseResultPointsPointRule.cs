namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal sealed class UseResultPointsPointRule : CalculationPointRuleBase
{
    public override IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows)
    {
        return rows;
    }
}
