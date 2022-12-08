namespace iRLeagueApiCore.Services.ResultService.Models;

internal sealed class StandingRowCalculationResult
{
    public StandingRowCalculationResult() { }
    public StandingRowCalculationResult(StandingRowCalculationResult data)
    {
        MemberId = data.MemberId;
        TeamId = data.TeamId;
        Position = data.Position;
        LastPosition = data.LastPosition;
        ClassId = data.ClassId;
        CarClass = data.CarClass;
        RacePoints = data.RacePoints;
        RacePointsChange = data.RacePointsChange;
        PenaltyPoints = data.PenaltyPoints;
        PenaltyPointsChange = data.PenaltyPointsChange;
        TotalPoints = data.TotalPoints;
        TotalPointsChange = data.TotalPointsChange;
        Races = data.Races;
        RacesCounted = data.RacesCounted;
        DroppedResultCount = data.DroppedResultCount;
        CompletedLaps = data.CompletedLaps;
        CompletedLapsChange = data.CompletedLapsChange;
        LeadLaps = data.LeadLaps;
        LeadLapsChange = data.LeadLapsChange;
        FastestLaps = data.FastestLaps;
        FastestLapsChange = data.FastestLapsChange;
        PolePositions = data.PolePositions;
        PolePositionsChange = data.PolePositionsChange;
        Wins = data.Wins;
        WinsChange = data.WinsChange;
        Top3 = data.Top3;
        Top5 = data.Top5;
        Top10 = data.Top10;
        Incidents = data.Incidents;
        IncidentsChange = data.IncidentsChange;
        PositionChange = data.PositionChange;
        ResultRows = data.ResultRows;
    }

    public long? MemberId { get; set; }
    public long? TeamId { get; set; }
    public int Position { get; set; }
    public int LastPosition { get; set; }
    public int ClassId { get; set; }
    public string CarClass { get; set; } = string.Empty;
    public int RacePoints { get; set; }
    public int RacePointsChange { get; set; }
    public int PenaltyPoints { get; set; }
    public int PenaltyPointsChange { get; set; }
    public int TotalPoints { get; set; }
    public int TotalPointsChange { get; set; }
    public int Races { get; set; }
    public int RacesCounted { get; set; }
    public int DroppedResultCount { get; set; }
    public int CompletedLaps { get; set; }
    public int CompletedLapsChange { get; set; }
    public int LeadLaps { get; set; }
    public int LeadLapsChange { get; set; }
    public int FastestLaps { get; set; }
    public int FastestLapsChange { get; set; }
    public int PolePositions { get; set; }
    public int PolePositionsChange { get; set; }
    public int Wins { get; set; }
    public int WinsChange { get; set; }
    public int Top3 { get; set; }
    public int Top5 { get; set; }
    public int Top10 { get; set; }
    public int Incidents { get; set; }
    public int IncidentsChange { get; set; }
    public int PositionChange { get; set; }

    public IList<ResultRowCalculationResult> ResultRows { get; set; } = new List<ResultRowCalculationResult>();
}
