using iRLeagueApiCore.Services.ResultService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal class DefaultPointRule<TRow> : PointRule<TRow>
    {
        private readonly IEnumerable<RowFilter<TRow>> filters = Array.Empty<RowFilter<TRow>>();

        public override IEnumerable<RowFilter<TRow>> PointFilters => filters;
        public override IEnumerable<RowFilter<TRow>> FinalFilters => filters;

        public override IReadOnlyList<TRow> ApplyPoints(IReadOnlyList<TRow> rows)
        {
            return rows;
        }

        public override IReadOnlyList<TRow> SortFinal(IEnumerable<TRow> rows)
        {
            if (rows is IReadOnlyList<TRow> list)
            {
                return list;
            }
            return rows.ToList();
        }

        public override IReadOnlyList<TRow> SortForPoints(IEnumerable<TRow> rows)
        {
            if (rows is IReadOnlyList<TRow> list)
            {
                return list;
            }
            return rows.ToList();
        }
    }
}
