using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueApiCore.Services.ResultService.Extensions;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal sealed class TeamSessionCalculationService : CalculationServiceBase
{
    private readonly SessionCalculationConfiguration config;

    public TeamSessionCalculationService(SessionCalculationConfiguration config)
    {
        this.config = config;
    }

    public override Task<SessionCalculationResult> Calculate(SessionCalculationData data)
    {
        var memberRows = data.ResultRows.Select(x => new ResultRowCalculationResult(x))
            .OrderBy(x => x.FinalPosition);
        var teamRows = memberRows
            .GroupBy(x => x.TeamId)
            .Where(x => x.Key != null)        
            .Select(x => GetTeamResultRow(x))
            .NotNull()
            .ToList();
        var pointRule = config.PointRule;
        var finalRows = ApplyPoints(teamRows, pointRule, data);

        var result = new SessionCalculationResult(data)
        {
            Name = config.Name,
            SessionResultId = config.SessionResultId,
            ResultRows = finalRows,
            SessionNr = data.SessionNr
        };

        return Task.FromResult(result);
    }

    private ResultRowCalculationResult? GetTeamResultRow(IEnumerable<ResultRowCalculationResult> teamMemberRows)
    {
        // 2. Keep two best driver results
        teamMemberRows = teamMemberRows.Take(config.MaxResultsPerGroup);
        if (teamMemberRows.Any() == false)
        {
            return null;
        }
        // 3. Accumulate results
        var teamRow = new ResultRowCalculationResult(teamMemberRows.First())
        {
            ScoredResultRowId = null,
            MemberId = null,
            Firstname = string.Empty,
            Lastname = string.Empty,
        };
        foreach (var memberRow in teamMemberRows.Skip(1))
        {
            teamRow.StartPosition = Math.Min(teamRow.StartPosition, memberRow.StartPosition);
            teamRow.CompletedLaps += memberRow.CompletedLaps;
            teamRow.LeadLaps += memberRow.LeadLaps;
            teamRow.Incidents += memberRow.Incidents;
            teamRow.Interval += memberRow.Interval;
            teamRow.RacePoints += memberRow.RacePoints;
            teamRow.BonusPoints += memberRow.BonusPoints;
            teamRow.PenaltyPoints += memberRow.PenaltyPoints;
        }
        (_, teamRow.QualifyingTime) = GetBestLapValue(teamMemberRows, x => x.MemberId, x => x.QualifyingTime);
        (_, teamRow.FastestLapTime) = GetBestLapValue(teamMemberRows, x => x.MemberId, x => x.FastestLapTime);
        teamRow.AvgLapTime = GetAverageLapValue(teamMemberRows, x => x.AvgLapTime, x => x.CompletedLaps);
        teamRow.ScoredMemberResultRowIds = teamMemberRows
            .Select(x => x.ScoredResultRowId)
            .NotNull()
            .ToList();

        return teamRow;
    }
}
