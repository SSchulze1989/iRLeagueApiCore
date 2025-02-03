using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Models;
using System.Globalization;
using System.Reflection;

namespace iRLeagueApiCore.Services.ResultService.Calculation.Filters;

internal sealed class ColumnValueRowFilter : RowFilter<ResultRowCalculationResult>
{
    /// <summary>
    /// Create an instance of a <see cref="ColumnValueRowFilter"/>
    /// </summary>
    /// <param name="propertyName">Name of the property matching the desired column</param>
    /// <param name="comparatorType">Type of comparison to execute</param>
    /// <param name="values">Values corresponding to comparator</param>
    /// <param name="action">Action to perform: Keep / Remove</param>
    /// <param name="allowForEach">Enable multiplication of rows using "ForEach" comparator</param>
    /// <exception cref="ArgumentException"></exception>
    public ColumnValueRowFilter(string propertyName, ComparatorType comparatorType, IEnumerable<string> values, MatchedValueAction action, bool allowForEach = false)
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
        Comparator = new(comparatorType, action, allowForEach);
    }

    public PropertyInfo ColumnProperty { get; }
    public ComparatorFilter Comparator { get; }
    public IEnumerable<IComparable?> FilterValues { get; }

    public override IEnumerable<T> FilterRows<T>(IEnumerable<T> rows)
    {
        return Comparator.GetFilteredRows(rows, (row) => (IComparable?)ColumnProperty.GetValue(row), FilterValues);
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
        if (type.IsEnum)
        {
            return values.Select(x => Enum.Parse(type, x)).Cast<IComparable>();
        }
        return values.Select(x => Convert.ChangeType(x, type, CultureInfo.InvariantCulture)).Cast<IComparable>();
    }
}
