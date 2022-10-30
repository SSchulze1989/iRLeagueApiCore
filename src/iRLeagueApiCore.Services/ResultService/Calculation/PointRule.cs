using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal abstract class PointRule<TRow>
    {
        public abstract IEnumerable<RowFilter<TRow>> PointFilters { get; }
        public abstract IEnumerable<RowFilter<TRow>> FinalFilters { get; }
        public abstract IReadOnlyList<TRow> SortForPoints(IEnumerable<TRow> rows);
        public abstract IReadOnlyList<TRow> ApplyPoints(IReadOnlyList<TRow> rows);
        public abstract IReadOnlyList<TRow> SortFinal(IEnumerable<TRow> rows);

        public static PointRule<TRow> Default() => new DefaultPointRule<TRow>();
    }
}