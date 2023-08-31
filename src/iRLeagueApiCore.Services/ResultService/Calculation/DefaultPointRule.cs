using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal class DefaultPointRule<TRow> : PointRule<TRow> where TRow : IPointRow, IPenaltyRow
{
    private readonly FilterGroupRowFilter<TRow> filters = new(Array.Empty<(FilterCombination, RowFilter<TRow>)>());

    public override FilterGroupRowFilter<TRow> GetPointFilters() => filters;
    public override FilterGroupRowFilter<TRow> GetResultFilters() => filters;
    public override FilterGroupRowFilter<TRow> GetChampSeasonFilters() => filters;

    public override IEnumerable<AutoPenaltyConfigurationData> GetAutoPenalties() => Array.Empty<AutoPenaltyConfigurationData>();

    public override IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows)
    {
        foreach (var row in rows)
        {
            row.TotalPoints = row.RacePoints + row.BonusPoints - row.PenaltyPoints;
        }
        return rows;
    }

    public override IReadOnlyList<T> SortFinal<T>(IEnumerable<T> rows) => DefaultPointRule<TRow>.DefaultSort(rows);

    public override IReadOnlyList<T> SortForPoints<T>(IEnumerable<T> rows) => DefaultPointRule<TRow>.DefaultSort(rows);

    public override IDictionary<string, int> GetBonusPoints() => new Dictionary<string, int>();

    private static IReadOnlyList<T> DefaultSort<T>(IEnumerable<T> rows) where T : TRow
    {
        if (rows is IReadOnlyList<T> list)
        {
            return list;
        }
        return rows.ToList();
    }
}
