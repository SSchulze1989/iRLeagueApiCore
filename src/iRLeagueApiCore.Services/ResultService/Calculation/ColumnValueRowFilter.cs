using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Models;
using System.Globalization;
using System.Reflection;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal class ColumnValueRowFilter : RowFilter<ResultRowCalculationResult>
{
    public ColumnValueRowFilter(string propertyName, ComparatorType comparator, IEnumerable<string> values, MatchedValueAction action)
    {
        try
        {
            ColumnProperty = GetColumnPropertyInfo(propertyName);
        }
        catch (InvalidOperationException ex)
        {
            throw new ArgumentException($"Parameter value {propertyName} did not target a valid column property on type {typeof(ResultRowCalculationResult)}",
                nameof(propertyName), ex);
        }
        CompareFunc = GetCompareFunction(comparator);
        try
        {
            FilterValues = GetFilterValuesOfType(ColumnProperty.PropertyType, values).ToList();
        }
        catch (Exception ex) when (ex is InvalidCastException || 
                                   ex is FormatException ||
                                   ex is OverflowException ||
                                   ex is ArgumentNullException)
        {
            throw new ArgumentException($"Parameter was not of type {ColumnProperty.PropertyType} of column property {ColumnProperty.Name}", nameof(values), ex);
        }
        Action = action;
    }

    private PropertyInfo ColumnProperty { get; set; }
    private Func<IComparable?, IEnumerable<IComparable>, bool> CompareFunc { get; set; }
    private IEnumerable<IComparable> FilterValues { get; set; } = Array.Empty<IComparable>();
    private MatchedValueAction Action { get; set; }

    public override IEnumerable<T> FilterRows<T>(IEnumerable<T> rows)
    {
        var match = rows.Where(x => MatchFilterValues(x, ColumnProperty, FilterValues, CompareFunc));
        return Action switch
        {
            MatchedValueAction.Keep => match,
            MatchedValueAction.Remove => rows.Except(match),
            _ => rows,
        };
    }

    private static bool MatchFilterValues(ResultRowCalculationResult row, PropertyInfo property, IEnumerable<IComparable> filterValues, Func<IComparable?, IEnumerable<IComparable>, bool> compare)
    {
        var value = (IComparable?)property.GetValue(row);
        return compare(value, filterValues);
    }

    private static PropertyInfo GetColumnPropertyInfo(string propertyName)
    {
        var sourceType = typeof(ResultRowCalculationResult);
        var propertyInfo = sourceType.GetProperty(propertyName)
            ?? throw new InvalidOperationException($"{typeof(ResultRowCalculationResult)} does not have a property with name {propertyName}");
        if (typeof(IComparable).IsAssignableFrom(propertyInfo.PropertyType) == false)
        {
            throw new InvalidOperationException($"Column {propertyName} of type {typeof(ResultRowCalculationResult)} does not implement IComparable");
        }
        return propertyInfo;
    }

    private static IEnumerable<IComparable> GetFilterValuesOfType(Type type, IEnumerable<string> values)
    {
        if (type.Equals(typeof(TimeSpan)))
        {
            return values.Select(x => TimeSpan.Parse(x)).Cast<IComparable>();
        }
        return values.Select(x => Convert.ChangeType(x, type, CultureInfo.InvariantCulture)).Cast<IComparable>();
    }

    private static Func<IComparable?, IEnumerable<IComparable>, bool> GetCompareFunction(ComparatorType comparatorType) 
        => comparatorType switch
    {
        ComparatorType.IsBigger => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 1; },
        ComparatorType.IsBiggerOrEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 1 || c == 0; },
        ComparatorType.IsEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 0; },
        ComparatorType.IsSmallerOrEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == -1 || c == 0; },
        ComparatorType.IsSmaller => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == -1; },
        ComparatorType.NotEqual => (x, y) => { var c = x?.CompareTo(y.FirstOrDefault()); return c == 1 || c == -1; },
        ComparatorType.InList => (x, y) => { var c = y.Any(z => x?.CompareTo(z) == 0); return c; },
        _ => (x, y) => true,
    };
}
