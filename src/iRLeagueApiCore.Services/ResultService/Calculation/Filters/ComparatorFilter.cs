using iRLeagueApiCore.Common.Enums;

namespace iRLeagueApiCore.Services.ResultService.Calculation.Filters;
internal sealed class ComparatorFilter
{
    public bool AllowForEach { get; private set; }
    public ComparatorType ComparatorType { get; }
    private Func<IComparable?, IEnumerable<IComparable?>, bool> CompareFunc { get; }
    public MatchedValueAction Action { get; }

    public ComparatorFilter(ComparatorType type, MatchedValueAction action, bool allowForEach = false)
    {
        ComparatorType = type;
        CompareFunc = GetCompareFunction(ComparatorType);
        Action = action;
        AllowForEach = allowForEach;
    }

    public IEnumerable<T> GetFilteredRows<T>(IEnumerable<T> rows, Func<T, IComparable?> getCompareValue, IEnumerable<IComparable?> filterValues)
    {
        if (rows.Any())
        {
            filterValues = ComparatorType switch
            {
                // Min and Max comaparators do not have fixed compare values - fetch value from row column
                ComparatorType.Min => [rows.Min(x => getCompareValue(x))],
                ComparatorType.Max => [rows.Max(x => getCompareValue(x))],
                // Use user provided values otherwise
                _ => filterValues,
            };
        }

        var match = rows.Where(x => MatchFilterValues(x, getCompareValue, filterValues, CompareFunc));
        if (ComparatorType == ComparatorType.ForEach && AllowForEach)
        {
            // special handling for ForEach --> duplicate rows by multiple of values
            match = MultiplyRows(match, getCompareValue, filterValues);
        }
        return Action switch
        {
            MatchedValueAction.Keep => match,
            MatchedValueAction.Remove => rows.Except(match),
            _ => rows,
        };
    }

    private static bool MatchFilterValues<T>(T row, Func<T, IComparable?> getCompareValue, IEnumerable<IComparable?> filterValues,
        Func<IComparable?, IEnumerable<IComparable?>, bool> compare)
    {
        var value = getCompareValue(row);
        return compare(value, filterValues);
    }

    private static IEnumerable<T> MultiplyRows<T>(IEnumerable<T> rows, Func<T, IComparable?> getCompareValue,
        IEnumerable<IComparable?> filterValues)
    {
        if (filterValues.Any() == false)
        {
            return rows;
        }
        var compareValue = Convert.ToDouble(filterValues.First());
        List<T> multipliedRows = [];
        foreach (var row in rows)
        {
            var value = Convert.ToDouble(getCompareValue(row));
            var count = (int)(value / compareValue);
            for (int i = 0; i < count; i++)
            {
                multipliedRows.Add(row);
            }
        }
        return multipliedRows;
    }

    private static Func<IComparable?, IEnumerable<IComparable?>, bool> GetCompareFunction(ComparatorType comparatorType)
        => comparatorType switch
        {
            ComparatorType.IsBigger => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 1; }
            ,
            ComparatorType.IsBiggerOrEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 1 || c == 0; }
            ,
            ComparatorType.IsEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 0; }
            ,
            ComparatorType.IsSmallerOrEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == -1 || c == 0; }
            ,
            ComparatorType.IsSmaller => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == -1; }
            ,
            ComparatorType.NotEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 1 || c == -1; }
            ,
            ComparatorType.InList => (x, y) => { var c = y.Any(z => x?.CompareTo(z) == 0); return c; }
            ,
            ComparatorType.ForEach => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 1 || c == 0; }
            ,
            ComparatorType.Max or ComparatorType.Min => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 0; }
            ,
            _ => (x, y) => true,
        };
}
