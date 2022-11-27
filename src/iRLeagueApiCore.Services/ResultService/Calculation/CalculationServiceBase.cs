using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

abstract internal class CalculationServiceBase : ICalculationService<SessionCalculationData, SessionCalculationResult>
{
    public abstract Task<SessionCalculationResult> Calculate(SessionCalculationData data);

    protected static IEnumerable<ResultRowCalculationResult> ApplyPointRule(IEnumerable<ResultRowCalculationResult> rows, PointRule<ResultRowCalculationResult> pointRule)
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
}
