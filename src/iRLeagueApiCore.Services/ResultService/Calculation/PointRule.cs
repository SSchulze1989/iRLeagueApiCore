using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal abstract class PointRule<TRow>
    {
        public abstract IEnumerable<RowFilter<TRow>> PointFilters { get; }
        public abstract IEnumerable<RowFilter<TRow>> FinalFilters { get; }
        public abstract IReadOnlyList<T> SortForPoints<T>(IEnumerable<T> rows) where T : TRow;
        public abstract IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows) where T : TRow;
        public abstract IReadOnlyList<T> SortFinal<T>(IEnumerable<T> rows) where T : TRow;

        public static PointRule<TRow> Default() => new DefaultPointRule<TRow>();
    }
}