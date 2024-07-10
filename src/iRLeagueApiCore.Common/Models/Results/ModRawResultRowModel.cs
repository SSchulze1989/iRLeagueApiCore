namespace iRLeagueApiCore.Common.Models.Results;

[DataContract]
public sealed class ModRawResultRowModel
{
    [DataMember]
    public double StartPosition { get; set; }
    [DataMember]
    public double FinishPosition { get; set; }
    [DataMember]
    public string CarNumber { get; set; } = string.Empty;
    [DataMember]
    public int ClassId { get; set; }
    [DataMember]
    public string Car { get; set; } = string.Empty;
    [DataMember]
    public string CarClass { get; set; } = string.Empty;
    [DataMember]
    public double CompletedLaps { get; set; }
    [DataMember]
    public double LeadLaps { get; set; }
    [DataMember]
    public int FastLapNr { get; set; }
    [DataMember]
    public double Incidents { get; set; }
    [DataMember]
    public int Status { get; set; }
    [DataMember]
    public TimeSpan QualifyingTime { get; set; }
    [DataMember]
    public TimeSpan Interval { get; set; }
    [DataMember]
    public TimeSpan AvgLapTime { get; set; }
    [DataMember]
    public TimeSpan FastestLapTime { get; set; }
    [DataMember]
    public double PositionChange { get; set; }
    [DataMember]
    public string IRacingId { get; set; } = string.Empty;
    [DataMember]
    public int SimSessionType { get; set; }
    [DataMember]
    public int OldIRating { get; set; }
    [DataMember]
    public int NewIRating { get; set; }
    [DataMember]
    public int SeasonStartIRating { get; set; }
    [DataMember]
    public string License { get; set; } = string.Empty;
    [DataMember]
    public double OldSafetyRating { get; set; }
    [DataMember]
    public double NewSafetyRating { get; set; }
    [DataMember]
    public int OldCpi { get; set; }
    [DataMember]
    public int NewCpi { get; set; }
    [DataMember]
    public int ClubId { get; set; }
    [DataMember]
    public string ClubName { get; set; } = string.Empty;
    [DataMember]
    public int CarId { get; set; }
    [DataMember]
    public double CompletedPct { get; set; }
    [DataMember]
    public DateTime? QualifyingTimeAt { get; set; }
    [DataMember]
    public int Division { get; set; }
    [DataMember]
    public int OldLicenseLevel { get; set; }
    [DataMember]
    public int NewLicenseLevel { get; set; }
    [DataMember]
    public int NumPitStops { get; set; }
    [DataMember]
    public string PittedLaps { get; set; } = string.Empty;
    [DataMember]
    public int NumOfftrackLaps { get; set; }
    [DataMember]
    public string OfftrackLaps { get; set; } = string.Empty;
    [DataMember]
    public int NumContactLaps { get; set; }
    [DataMember]
    public string ContactLaps { get; set; } = string.Empty;
    [DataMember]
    public double RacePoints { get; set; }
    [DataMember]
    public long? TeamId { get; set; }
    [DataMember]
    public bool PointsEligible { get; set; }
}
