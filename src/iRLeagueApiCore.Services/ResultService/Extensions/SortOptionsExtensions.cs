using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Extensions
{
    internal static class SortOptionsExtensions
    {
        public static Func<T, object> GetSortingValue<T>(this SortOptions sortOption) where T : ResultRowCalculationData
        {
            return sortOption switch
            {
                SortOptions.BonusPtsAsc => row => row.BonusPoints,
                SortOptions.BonusPtsDesc => row => -row.BonusPoints,
                SortOptions.ComplLapsAsc => row => row.CompletedLaps,
                SortOptions.ComplLapsDesc => row => -row.CompletedLaps,
                SortOptions.FastLapAsc => row => GetLapTimeSortValue(row.FastestLapTime),
                SortOptions.FastLapDesc => row => -GetLapTimeSortValue(row.FastestLapTime),
                SortOptions.IncsAsc => row => row.Incidents,
                SortOptions.IncsDesc => row => -row.Incidents,
                SortOptions.IntvlAsc => row => row.Interval,
                SortOptions.IntvlDesc => row => -row.Interval,
                SortOptions.LeadLapsAsc => row => row.LeadLaps,
                SortOptions.LeadLapsDesc => row => -row.LeadLaps,
                SortOptions.PenPtsAsc => row => row.PenaltyPoints,
                SortOptions.PenPtsDesc => row => -row.PenaltyPoints,
                SortOptions.PosAsc => row => row.FinishPosition,
                SortOptions.PosDesc => row => -row.FinishPosition,
                SortOptions.QualLapAsc => row => GetLapTimeSortValue(row.QualifyingTime),
                SortOptions.QualLapDesc => row => -GetLapTimeSortValue(row.QualifyingTime),
                SortOptions.RacePtsAsc => row => row.RacePoints,
                SortOptions.RacePtsDesc => row => -row.RacePoints,
                SortOptions.StartPosAsc => row => row.StartPosition,
                SortOptions.StartPosDesc => row => -row.StartPosition,
                SortOptions.TotalPtsAsc => row => row.TotalPoints,
                SortOptions.TotalPtsDesc => row => -row.TotalPoints,
                _ => row => 0,
            };
        }

        private static TimeSpan GetLapTimeSortValue(TimeSpan lapTime)
        {
            return lapTime != TimeSpan.Zero ? lapTime : TimeSpan.MaxValue;
        }
    }
}
