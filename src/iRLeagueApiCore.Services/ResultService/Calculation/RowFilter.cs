using iRLeagueApiCore.Services.ResultService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal abstract class RowFilter<TRow>
    {
        public abstract IEnumerable<TRow> FilterRows(IEnumerable<TRow> rows);

        public static RowFilter<TRow> Default() => new DefaultRowFilter<TRow>();
    }
}
