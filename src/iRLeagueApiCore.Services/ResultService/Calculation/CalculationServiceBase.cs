using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

abstract internal class CalculationServiceBase : ICalculationService<SessionCalculationData, SessionCalculationResult>
{
    public abstract Task<SessionCalculationResult> Calculate(SessionCalculationData data);

    protected static IEnumerable<ResultRowCalculationResult> ApplyPoints(IEnumerable<ResultRowCalculationResult> rows, PointRule<ResultRowCalculationResult> pointRule,
        SessionCalculationData data)
    {
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
        foreach (var filter in pointRule.GetResultFilters())
        {
            finalRows = filter.FilterRows(finalRows);
        }
        finalRows = ApplyReviewPenalties(finalRows, data.AcceptedReviewVotes);
        finalRows = ApplyTotalPoints(finalRows);
        finalRows = pointRule.SortFinal(finalRows);
        // Set final position
        foreach ((var row, var position) in finalRows.Select((x, i) => (x, i + 1)))
        {
            row.FinalPosition = position;
            row.FinalPositionChange = row.StartPosition - row.FinalPosition;
        }

        return finalRows;
    }

    protected static (long? id, TimeSpan lap) GetBestLapValue<T>(IEnumerable<T> rows, Func<T, long?> idSelector, Func<T, TimeSpan> valueSelector)
    {
        return rows
            .Select(row => ((long? id, TimeSpan lap))(idSelector.Invoke(row), valueSelector.Invoke(row)))
            .Where(row => LapIsValid(row.lap))
            .DefaultIfEmpty()
            .MinBy(row => row.lap);
    }

    protected static IEnumerable<(long? id, TValue value)> GetBestValues<T, TValue>(IEnumerable<T> rows, Func<T, TValue> valueSelector, Func<T, long?> idSelector,
        Func<IEnumerable<TValue>, TValue> bestValueFunc, EqualityComparer<TValue>? comparer = default)
    {
        comparer ??= EqualityComparer<TValue>.Default;
        var valueRows = rows.Select(row => ((long? id, TValue value))(idSelector.Invoke(row), valueSelector.Invoke(row)));
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
        return true;
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
        var groupedRows = rows.GroupBy(groupBy);
        var combined = new List<ResultRowCalculationResult>();

        foreach(var group in groupedRows.Where(x => x.Any()))
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
}
