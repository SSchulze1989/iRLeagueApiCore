namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class SessionResultCalculationResult
    {
        public long LeagueId { get; set; }
        public long? SessionId { get; set; }
        public long? SessionResultId { get; set; }
        public long? ScoringId { get; set; }
        public string Name { get; set; } = string.Empty;
        public long FastestLap { get; set; }
        public long FastestQualyLap { get; set; }
        public long FastestAvgLap { get; set; }
        public long? FastestAvgLapDriverMemberId { get; set; }
        public long? FastestLapDriverMemberId { get; set; }
        public long? FastestQualyLapDriverMemberId { get; set; }
        public ICollection<long> CleanestDrivers { get; set; } = Array.Empty<long>();
        public ICollection<long> HardChargers { get; set; } = Array.Empty<long>();
        public IEnumerable<ResultRowCalculationResult> ResultRows { get; set; } = Array.Empty<ResultRowCalculationResult>();
    }
}
