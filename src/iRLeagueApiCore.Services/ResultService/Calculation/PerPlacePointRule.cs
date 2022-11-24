using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class PerPlacePointRule : CalculationPointRuleBase
    {
        public IReadOnlyDictionary<int, double> PointsPerPlace { get; private set; }

        public PerPlacePointRule(IReadOnlyDictionary<int, double> pointPerPlace)
        {
            PointsPerPlace = pointPerPlace;
        }

        public override IReadOnlyList<T> ApplyPoints<T>(IReadOnlyList<T> rows)
        {
            foreach((var row, var pos) in rows.Select((x, i) => (x, i + 1)))
            {
                row.RacePoints = PointsPerPlace.TryGetValue(pos, out double points) ? points : 0d;
                row.PenaltyPoints += row.PenaltyPoints + row.AddPenalty?.PenaltyPoints ?? 0;
                row.TotalPoints = row.RacePoints + row.BonusPoints - row.PenaltyPoints;
            }
            return rows;
        }
    }
}
