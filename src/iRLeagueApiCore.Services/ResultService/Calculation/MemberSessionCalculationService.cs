using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.ResultService.Extensions;

namespace iRLeagueApiCore.Services.ResultService.Calculation
{
    internal sealed class MemberSessionCalculationService : ICalculationService<SessionCalculationData, SessionCalculationResult>
    {
        private readonly SessionCalculationConfiguration config;

        public MemberSessionCalculationService(SessionCalculationConfiguration config)
        {
            this.config = config;
        }

        public Task<SessionCalculationResult> Calculate(SessionCalculationData data)
        {
            if (data.LeagueId != config.LeagueId) 
                throw new InvalidOperationException("LeagueId does not match");
            if (config.SessionId != null && data.SessionId != config.SessionId) 
                throw new InvalidOperationException("SessionId does not match");

            var rows = data.ResultRows
                .Select(row => new ResultRowCalculationResult(row))
                .Where(row => row.MemberId != null)
                .ToList();
            var pointRows = rows.AsEnumerable();
            var pointRule = config.PointRule;
            
            // Filter for points only
            foreach(var filter in pointRule.GetPointFilters())
            {
                pointRows = filter.FilterRows(pointRows);
            }
            
            // Calculation
            var calcRows = pointRule.SortForPoints(pointRows);
            pointRule.ApplyPoints(calcRows);

            var finalRows = rows.AsEnumerable();
            foreach(var filter in pointRule.GetFinalFilters())
            {
                finalRows = filter.FilterRows(finalRows);
            }
            finalRows = pointRule.SortFinal(finalRows);
            // Set final position
            foreach((var row, var position) in finalRows.Select((x, i) => (x, i+1)))
            {
                row.FinalPosition = position;
                row.FinalPositionChange = row.StartPosition - row.FinalPosition;
            }

            var sessionResult = new SessionCalculationResult(data);
            sessionResult.SessionResultId = config.SessionResultId;
            sessionResult.ResultRows = finalRows;
            (sessionResult.FastestAvgLapDriverMemberId, sessionResult.FastestAvgLap) = GetBestLapValue(finalRows, x => x.MemberId, x => x.AvgLapTime);
            (sessionResult.FastestLapDriverMemberId, sessionResult.FastestLap) = GetBestLapValue(finalRows, x => x.MemberId, x => x.FastestLapTime);
            (sessionResult.FastestQualyLapDriverMemberId, sessionResult.FastestQualyLap) = GetBestLapValue(finalRows, x => x.MemberId, x => x.QualifyingTime);
            sessionResult.CleanestDrivers = GetBestValues(rows, x => x.Incidents, x => x.MemberId, x => x.Min())
                .Select(x => x.id)
                .NotNull()
                .ToList();
            sessionResult.HardChargers = GetBestValues(rows.Where(HardChargerEligible), x => x.FinalPositionChange, x => x.MemberId, x => x.Max())
                .Select(x => x.id)
                .NotNull()
                .ToList();

            return Task.FromResult(sessionResult);
        }

        private static (long? id, TimeSpan lap) GetBestLapValue<T>(IEnumerable<T> rows, Func<T, long?> idSelector, Func<T, TimeSpan> valueSelector)
        {
            return rows
                .Select(row =>((long? id, TimeSpan lap))(idSelector.Invoke(row), valueSelector.Invoke(row)))
                .Where(row => LapIsValid(row.lap))
                .MinBy(row => row.lap);
        }

        private static IEnumerable<(long? id, TValue value)> GetBestValues<T, TValue>(IEnumerable<T> rows, Func<T, TValue> valueSelector, Func<T, long?> idSelector,
            Func<IEnumerable<TValue>, TValue> bestValueFunc, EqualityComparer<TValue>? comparer = default)
        {
            comparer ??= EqualityComparer<TValue>.Default;
            var valueRows = rows.Select(row => ((long? id, TValue value))(idSelector.Invoke(row), valueSelector.Invoke(row)));
            var bestValue = bestValueFunc.Invoke(valueRows.Select(x => x.value));
            return valueRows.Where(row => comparer.Equals(row.value, bestValue));
        }

        private static bool HardChargerEligible(ResultRowCalculationResult row)
        {
            return true;
        }

        private static bool LapIsValid(TimeSpan lap)
        {
            return lap > TimeSpan.Zero;
        }
    }
}