﻿namespace iRLeagueApiCore.Services.ResultService.Calculation.Filters;

internal sealed class DefaultRowFilter<TRow> : RowFilter<TRow>
{
    public override IEnumerable<T> FilterRows<T>(IEnumerable<T> rows)
    {
        return rows;
    }
}
