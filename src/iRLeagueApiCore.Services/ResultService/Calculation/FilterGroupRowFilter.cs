namespace iRLeagueApiCore.Services.ResultService.Calculation;
internal sealed class FilterGroupRowFilter<T> : RowFilter<T>
{
    private IList<(FilterCombination combination, RowFilter<T> rowFilter)> filters;

    public FilterGroupRowFilter(IEnumerable<(FilterCombination, RowFilter<T>)> filters)
    {
        this.filters = filters.ToList();
    }

    public override IEnumerable<TRow> FilterRows<TRow>(IEnumerable<TRow> rows)
    {
        var originalRows = rows;
        foreach(var (combination, filter) in filters)
        {
            rows = combination switch
            {
                FilterCombination.And => filter.FilterRows(rows),
                FilterCombination.Or => rows.Union(filter.FilterRows(originalRows)),
                _ => rows,
            };
        }
        return originalRows.Where(x => rows.Contains(x));
    }
}

public enum FilterCombination
{
    And,
    Or,
}
