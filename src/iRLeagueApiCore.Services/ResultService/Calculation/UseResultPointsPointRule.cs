namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal sealed class UseResultPointsPointRule : CalculationPointRuleBase
{
    public override IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows)
    {
        foreach (var row in rows)
        {
            row.PenaltyPoints = row.AddPenalty?.PenaltyPoints ?? 0;
            row.TotalPoints = row.RacePoints + row.BonusPoints - row.PenaltyPoints;
        }
        return rows;
    }
}
