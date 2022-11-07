using iRLeagueApiCore.Services.ResultService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal class DefaultPointRule<TRow> : PointRule<TRow> where TRow : IPointRow, IPenaltyRow
    {
        private readonly IEnumerable<RowFilter<TRow>> filters = Array.Empty<RowFilter<TRow>>();

        public override IEnumerable<RowFilter<TRow>> GetPointFilters() => filters;
        public override IEnumerable<RowFilter<TRow>> GetFinalFilters() => filters;

        public override IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows)
        {
            foreach(var row in rows)
            {
                row.PenaltyPoints = row.AddPenalty?.PenaltyPoints ?? 0;
                row.TotalPoints = row.RacePoints + row.BonusPoints - row.PenaltyPoints;
            }
            return rows;
        }

        public override IReadOnlyList<T> SortFinal<T>(IEnumerable<T> rows) => DefaultPointRule<TRow>.DefaultSort(rows);

        public override IReadOnlyList<T> SortForPoints<T>(IEnumerable<T> rows) => DefaultPointRule<TRow>.DefaultSort(rows);

        private static IReadOnlyList<T> DefaultSort<T>(IEnumerable<T> rows) where T : TRow
        {
            if (rows is IReadOnlyList<T> list)
            {
                return list;
            }
            return rows.ToList();
        }
    }
}
