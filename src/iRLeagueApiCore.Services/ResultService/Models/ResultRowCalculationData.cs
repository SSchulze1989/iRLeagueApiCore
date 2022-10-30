using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class ResultRowCalculationData
    {
        public long? MemberId { get; set; }
        public string Firstname { get; set; } = string.Empty;
        public string Lastname { get; set; } = string.Empty;
        public long? TeamId { get; set; }
        public string TeamName { get; set; } = string.Empty;
        public double StartPosition { get; set; }
        public double FinishPosition { get; set; }
        public int CarNumber { get; set; }
        public int ClassId { get; set; }
        public string Car { get; set; } = string.Empty;
        public string CarClass { get; set; } = string.Empty;
        public double CompletedLaps { get; set; }
        public double LeadLaps { get; set; }
        public int FastLapNr { get; set; }
        public double Incidents { get; set; }
        public int Status { get; set; }
        public TimeSpan QualifyingTime { get; set; }
        public TimeSpan Interval { get; set; }
        public TimeSpan AvgLapTime { get; set; }
        public TimeSpan FastestLapTime { get; set; }
        public double PositionChange { get; set; }
        public int OldIrating { get; set; }
        public int NewIrating { get; set; }
        public int SeasonStartIrating { get; set; }
        public string License { get; set; } = string.Empty;
        public double OldSafetyRating { get; set; }
        public double NewSafetyRating { get; set; }
        public int CarId { get; set; }
        public double? CompletedPct { get; set; }
        public int Division { get; set; }
        public int OldLicenseLevel { get; set; }
        public int NewLicenseLevel { get; set; }
        public double RacePoints { get; set; }
        public double BonusPoints { get; set; }
        public double PenaltyPoints { get; set; }
        public double TotalPoints { get; set; }
        public int FinalPosition { get; set; }
        public double FinalPositionChange { get; set; }
        public AddPenaltyCalculationData? AddPenalty { get; set; }
    }
}
