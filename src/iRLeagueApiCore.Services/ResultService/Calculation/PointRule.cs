using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal abstract class PointRule<TRow> where TRow : IPointRow, IPenaltyRow
    {
        public abstract IEnumerable<RowFilter<TRow>> GetPointFilters();
        public abstract IEnumerable<RowFilter<TRow>> GetResultFilters();
        public abstract IReadOnlyList<T> SortForPoints<T>(IEnumerable<T> rows) where T : TRow;
        public abstract IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows) where T : TRow;
        public abstract IReadOnlyList<T> SortFinal<T>(IEnumerable<T> rows) where T : TRow;

        private readonly static DefaultPointRule<TRow> defaultPointRule = new DefaultPointRule<TRow>();
        public static PointRule<TRow> Default() => defaultPointRule;
    }
}