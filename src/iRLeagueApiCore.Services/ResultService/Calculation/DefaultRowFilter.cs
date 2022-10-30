namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class DefaultRowFilter<TRow> : RowFilter<TRow>
    {
        public override IEnumerable<TRow> FilterRows(IEnumerable<TRow> rows)
        {
            return rows;
        }
    }
}