using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation.Filters;

/// <summary>
/// Filter that counts the number of rows in the filter input and either matches all rows if the conditions match or none
/// </summary>
internal sealed class CountRowFilter : RowFilter<ResultRowCalculationResult>
{
    public ComparatorFilter Comparator { get; }
    public IEnumerable<IComparable?> FilterValues { get; }

    public CountRowFilter(ComparatorType comparator, IEnumerable<string> filterValues, MatchedValueAction action, bool allowForEach = false)
    {
        Comparator = new(comparator, action, allowForEach);
        try
        {
            FilterValues = filterValues.Select(x => Convert.ToInt32(x)).Cast<IComparable?>().ToList();
        }
        catch (Exception ex) when (ex is InvalidCastException ||
                                   ex is FormatException ||
                                   ex is OverflowException)
        {
            throw new ArgumentException($"Parameter was not of type {typeof(int)}", nameof(filterValues), ex);
        }
    }

    public override IEnumerable<T> FilterRows<T>(IEnumerable<T> rows)
    {
        int count = rows.Count();
        return Comparator.GetFilteredRows(rows, _ => count, FilterValues);
    }
}
