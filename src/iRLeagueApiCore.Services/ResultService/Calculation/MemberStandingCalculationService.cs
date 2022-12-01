using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal sealed class MemberStandingCalculationService : ICalculationService<StandingCalculationData, StandingCalculationResult>
{
    private readonly StandingCalculationConfiguration config;

    public MemberStandingCalculationService(StandingCalculationConfiguration config)
    {
        this.config = config;
    }

    public Task<StandingCalculationResult> Calculate(StandingCalculationData data)
    {
        // TODO:
        // 0. Get counting results per event
        //   -> if "combined" use single combined result
        //   -> if not "combined" use every race session result separately
        // 1. Group by memberId
        // 2. SortBy RacePoints
        // 3. Take top {weeksCounted} results
        // 4. Accumulate result rows
        // 5. Sort by total points then by penalty points
        // 6. Statistics? ...
        IEnumerable<SessionCalculationResult> previousSessionResults;
        if (config.UseCombinedResult)
        {
            data.PreviousEventResults.Select(x =>
                x.SessionResults.OrderBy(x => x.SessionNr).LastOrDefault())
                .NotNull();
        }
        var groupedMemberResultRows = data.PreviousEventResults.SelectMany() .SessionResults
            .SelectMany(result => result.ResultRows.Select(row => (result, row)))
            .OrderBy(x => x.row.RacePoints)
            .GroupBy(x => x.row.MemberId);
        foreach(var memberRows in groupedMemberResultRows)
        {
            var countedRows = memberRows.Take(config.WeeksCounted);
            var memberStandingRow = 
        }
    }

    private static StandingRowCalculationResult CalculateMemberStanding(
        IEnumerable<(SessionCalculationResult result, ResultRowCalculationResult row)> results)
    {
        var standingRow = new StandingRowCalculationResult();
        if (results.Any() == false)
        {
            return standingRow;
        }
        var firstResult = results.First();
        var firstRow = firstResult.row;
        // static data
        standingRow.CarClass = firstRow.CarClass;
        standingRow.ClassId = firstRow.ClassId;
        standingRow.MemberId = firstRow.MemberId;

        // accumulate rows
        foreach(var resultRow in results)
        {
            var result = resultRow.result;
            var row = resultRow.row;
            standingRow.CompletedLaps += (int)row.CompletedLaps;
            standingRow.FastestLaps += result.FastestLapDriverMemberId == standingRow.MemberId ? 1 : 0;
            standingRow.Incidents += (int)row.Incidents;
            standingRow.LeadLaps += (int)row.LeadLaps;
            standingRow.PenaltyPoints += (int)row.PenaltyPoints;
            standingRow.PolePositions += result.FastestQualyLapDriverMemberId == standingRow.MemberId ? 1 : 0;
            standingRow.RacePoints += (int)row.RacePoints;
            standingRow.RacesCounted += 1;
            standingRow.Top10 += row.FinalPosition <= 10 ? 1 : 0;
            standingRow.Top5 += row.FinalPosition <= 5 ? 1 : 0;
            standingRow.Top3 += row.FinalPosition <= 3 ? 1 : 0;
            standingRow.TotalPoints += (int)row.TotalPoints;
            standingRow.Wins += row.FinalPosition == 1 ? 1 : 0;
        }

        return standingRow;
    }
}
