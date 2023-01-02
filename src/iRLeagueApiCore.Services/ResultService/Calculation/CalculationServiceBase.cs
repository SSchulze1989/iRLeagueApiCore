using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

abstract internal class CalculationServiceBase : ICalculationService<SessionCalculationData, SessionCalculationResult>
{
    public abstract Task<SessionCalculationResult> Calculate(SessionCalculationData data);

    protected static IEnumerable<ResultRowCalculationResult> ApplyPoints(IEnumerable<ResultRowCalculationResult> rows, PointRule<ResultRowCalculationResult> pointRule,
        SessionCalculationData data)
    {
        foreach (var filter in pointRule.GetResultFilters())
        {
            rows = filter.FilterRows(rows);
        }

        IEnumerable<ResultRowCalculationResult> pointRows = rows.ToList();
        // Filter for points only
        foreach (var filter in pointRule.GetPointFilters())
        {
            pointRows = filter.FilterRows(pointRows);
        }

        // Calculation
        var calcRows = pointRule.SortForPoints(pointRows);
        pointRule.ApplyPoints(calcRows);

        IEnumerable<ResultRowCalculationResult> finalRows = rows;
        ApplyReviewPenalties(finalRows, data.AcceptedReviewVotes);
        ApplyBonusPoints(pointRows, pointRule.GetBonusPoints());
        ApplyTotalPoints(finalRows);
        finalRows = pointRule.SortFinal(finalRows);
        // Set final position
        foreach ((var row, var position) in finalRows.Select((x, i) => (x, i + 1)))
        {
            row.FinalPosition = position;
            row.FinalPositionChange = row.StartPosition - row.FinalPosition;
        }

        return finalRows;
    }

    protected static (TId? id, TimeSpan lap) GetBestLapValue<T, TId>(IEnumerable<T> rows, Func<T, TId?> idSelector, Func<T, TimeSpan> valueSelector)
    {
        return rows
            .Select(row => ((TId? id, TimeSpan lap))(idSelector.Invoke(row), valueSelector.Invoke(row)))
            .Where(row => LapIsValid(row.lap))
            .DefaultIfEmpty()
            .MinBy(row => row.lap);
    }

    protected static IEnumerable<(TId? id, TValue value)> GetBestValues<T, TValue, TId>(IEnumerable<T> rows, Func<T, TValue> valueSelector, Func<T, TId?> idSelector,
        Func<IEnumerable<TValue>, TValue> bestValueFunc, EqualityComparer<TValue>? comparer = default)
    {
        comparer ??= EqualityComparer<TValue>.Default;
        var valueRows = rows.Select(row => ((TId? id, TValue value))(idSelector.Invoke(row), valueSelector.Invoke(row)));
        var bestValue = bestValueFunc.Invoke(valueRows.Select(x => x.value));
        return valueRows.Where(row => comparer.Equals(row.value, bestValue));
    }

    protected static TimeSpan GetAverageLapValue<T>(IEnumerable<T> rows, Func<T, TimeSpan> valueSelector, Func<T, double> weightSelector)
    {
        double total = 0;
        TimeSpan lap = TimeSpan.Zero;
        foreach (var row in rows)
        {
            var w = weightSelector(row);
            total += w;
            lap += valueSelector(row) * w;
        }

        return lap / (total > 0 ? total : 1);
    }

    protected static bool LapIsValid(TimeSpan lap)
    {
        return lap > TimeSpan.Zero;
    }

    protected static bool HardChargerEligible(ResultRowCalculationResult row)
    {
        return row.QualifyingTime > TimeSpan.Zero;
    }

    /// <summary>
    /// Group and combine result rows using the given groupBy key selector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rows">rows to combine</param>
    /// <param name="groupBy">selector for the key by which to group the rows</param>
    /// <returns></returns>
    protected static IEnumerable<ResultRowCalculationResult> CombineResults<T>(IEnumerable<ResultRowCalculationResult> rows, Func<ResultRowCalculationResult, T> groupBy)
    {
        var groupedRows = rows.Reverse().GroupBy(groupBy);
        var combined = new List<ResultRowCalculationResult>();

        foreach (var group in groupedRows.Where(x => x.Any()))
        {
            var row = new ResultRowCalculationResult(group.First());
            foreach (var groupRow in group.Skip(1))
            {
                row.BonusPoints += groupRow.BonusPoints;
                row.CompletedLaps += groupRow.CompletedLaps;
                row.Incidents += groupRow.Incidents;
                row.LeadLaps += groupRow.LeadLaps;
                row.PenaltyPoints += groupRow.PenaltyPoints;
                row.RacePoints += groupRow.RacePoints;
            }
            row.AvgLapTime = GetAverageLapValue(group, x => x.AvgLapTime, x => x.CompletedLaps);
            (_, row.FastestLapTime) = GetBestLapValue(group, x => x.MemberId, x => x.FastestLapTime);
            (_, row.QualifyingTime) = GetBestLapValue(group, x => x.MemberId, x => x.QualifyingTime);
            row.FastLapNr = 0;
            var last = group.Last();
            row.NewCpi = last.NewCpi;
            row.NewIrating = last.NewIrating;
            row.NewLicenseLevel = last.NewLicenseLevel;
            row.NewSafetyRating = last.NewSafetyRating;
            combined.Add(row);
        }

        return combined;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyReviewPenalties(IEnumerable<ResultRowCalculationResult> rows, IEnumerable<AcceptedReviewVoteCalculationData> reviewVotes)
    {
        foreach (var row in rows)
        {
            var rowVotes = reviewVotes.Where(x => x.MemberAtFaultId == row.MemberId);
            foreach (var vote in rowVotes)
            {
                var penalty = new ReviewPenaltyCalculationResult()
                {
                    ReviewId = vote.ReviewId,
                    ReviewVoteId = vote.ReviewVoteId,
                    PenaltyPoints = vote.DefaultPenalty,
                };
                row.ReviewPenalties.Add(penalty);
                row.PenaltyPoints += penalty.PenaltyPoints;
            }
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyTotalPoints(IEnumerable<ResultRowCalculationResult> rows)
    {
        foreach (var row in rows)
        {
            row.TotalPoints = row.RacePoints + row.BonusPoints - row.PenaltyPoints;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyBonusPoints(IEnumerable<ResultRowCalculationResult> rows, IDictionary<string, int> BonusPoints)
    {
        if (rows.None())
        {
            return rows;
        }

        var minIncs = rows.Any(x => x.PenaltyPoints == 0) ? rows.Where(x => x.PenaltyPoints == 0).Min(x => x.Incidents) : -1;
        var fastestLapRow = GetBestLapValue(rows, x => x, x => x.FastestLapTime);

        foreach (var bonus in BonusPoints)
        {
            var bonusKey = bonus.Key[0];
            int bonusKeyValue = 0;
            int bonusPoints = bonus.Value;
            if (bonus.Key.Length > 1 && int.TryParse(bonus.Key[1..], out bonusKeyValue) == false)
            {
                continue;
            }
            switch (bonusKey)
            {
                case 'p':
                    ApplyPositionBonusPoints(rows, bonusKeyValue, bonusPoints);
                    break;
                case 'c':
                    ApplyCleanestDriverBonusPoints(rows, bonusPoints);
                    break;
                case 'f':
                    ApplyFastestLapBonusPoints(rows, bonusPoints);
                    break;
            };
        }

        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyPositionBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int position, int points)
    {
        foreach (var row in rows)
        {
            if (row.FinalPosition == position)
            {
                row.BonusPoints += points;
            }
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyCleanestDriverBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        static bool condition(ResultRowCalculationResult x) => x.PenaltyPoints == 0;
        var minIncRows = GetBestValues(rows.Where(condition), x => x.Incidents, x => x, x => x.Min())
            .Select(x => x.id)
            .NotNull();
        foreach (var row in minIncRows)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyFastestLapBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var (row, lapTime) = GetBestLapValue(rows, x => x, x => x.FastestLapTime);
        if (row is not null)
        {
            row.BonusPoints += points;
        }
        return rows;
    }
}
