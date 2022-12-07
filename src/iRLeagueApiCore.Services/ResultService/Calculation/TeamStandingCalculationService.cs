using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal sealed class TeamStandingCalculationService : ICalculationService<StandingCalculationData, StandingCalculationResult>
{
    private readonly StandingCalculationConfiguration config;

    public TeamStandingCalculationService(StandingCalculationConfiguration config)
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
        IEnumerable<(EventCalculationResult eventResult, IEnumerable<SessionCalculationResult> sessionResults)> previousSessionResults;
        (EventCalculationResult eventResult, IEnumerable<SessionCalculationResult> sessionResults) currentSessionResults;
        if (config.UseCombinedResult)
        {
            previousSessionResults = data.PreviousEventResults
                .Select(eventResult => (eventResult, eventResult.SessionResults.OrderBy(x => x.SessionNr).TakeLast(1)));
            currentSessionResults = (data.CurrentEventResult, data.CurrentEventResult.SessionResults.OrderBy(x => x.SessionNr).TakeLast(1));
        }
        else
        {
            previousSessionResults = data.PreviousEventResults
                .Select(eventResult => (eventResult, eventResult.SessionResults.Where(x => x.Name != "Practice" && x.Name != "Qualifying")));
            currentSessionResults = (data.CurrentEventResult,
                data.CurrentEventResult.SessionResults.Where(x => x.Name != "Practice" && x.Name != "Qualifying"));
        }

        // flatten results so that we get one entry for each single result row from previous events
        var previousResultRows = previousSessionResults
            .SelectMany(result => result.sessionResults
                .SelectMany(sessionResult => sessionResult.ResultRows
                    .Select(resultRow => (result.eventResult, sessionResult, resultRow))));

        // get the previous result rows for each individual driver
        var previousTeamResultRows = previousResultRows
            .Where(x => x.resultRow.TeamId is not null)
            .GroupBy(x => x.resultRow.TeamId!.Value);

        // get the previous result rows per event
        var previousTeamEventResults = previousTeamResultRows
            .Select(x => (key: x.Key, results: x
                .GroupBy(result => result.eventResult, result => new TeamSessionResultRow(x.Key, result.sessionResult, result.resultRow))
                .Select(result => new TeamEventResult(x.Key, result.Key, result))))
            .ToDictionary(k => k.key, v => v.results);

        // do the same for current event result
        var currentResultRows = currentSessionResults.sessionResults
            .SelectMany(sessionResult => sessionResult.ResultRows
                    .Select(resultRow => (sessionResult, resultRow)));
        var currentTeamResultRows = currentResultRows
            .Where(x => x.resultRow.TeamId != null)
            .GroupBy(x => x.resultRow.TeamId!.Value);
        var currentTeamEventResult = currentTeamResultRows.Select(x => (key: x.Key, eventResult: currentSessionResults.eventResult,
                sessionResults: x.Select(sessionResult => new TeamSessionResultRow(x.Key, sessionResult.sessionResult, sessionResult.resultRow))))
            .ToDictionary(k => k.key, v => new TeamEventResult(v.key, v.eventResult, v.sessionResults));

        var teamIds = previousTeamEventResults.Keys.Concat(currentTeamEventResult.Keys).Distinct();

        List<(long TeamId, StandingRowCalculationResult Previous, StandingRowCalculationResult Current)> memberStandingRows = new();
        foreach (var memberId in teamIds)
        {
            // sort by best race points each event 
            var previousEventResults = (previousTeamEventResults.GetValueOrDefault(memberId) ?? Array.Empty<TeamEventResult>())
                .OrderByDescending(GetEventOrderValue);
            var currentResult = currentTeamEventResult.GetValueOrDefault(memberId);
            var standingRow = new StandingRowCalculationResult();
            var lastResult = currentResult ?? previousEventResults.FirstOrDefault();
            var lastRow = lastResult?.SessionResults.LastOrDefault()?.ResultRow;
            if (lastRow is null)
            {
                continue;
            }
            // static data
            standingRow.MemberId = lastRow.MemberId;
            standingRow.CarClass = lastRow.CarClass;
            standingRow.ClassId = lastRow.ClassId;
            standingRow.TeamId = lastRow.TeamId;

            // accumulated data
            var previousStandingRow = new StandingRowCalculationResult(standingRow);

            var previousResults = previousEventResults.SelectMany(x => x.SessionResults);
            var countedEventResults = previousEventResults.Take(config.WeeksCounted);
            var countedSessionResults = countedEventResults.SelectMany(x => x.SessionResults);
            previousStandingRow = AccumulateOverallSessionResults(previousStandingRow, previousResults);
            previousStandingRow = AccumulateCountedSessionResults(previousStandingRow, countedSessionResults);

            if (currentResult is not null)
            {
                var currentResults = previousEventResults.Concat(new[] { currentResult })
                    .OrderByDescending(GetEventOrderValue);
                var currentMemberSessionResults = currentResults.SelectMany(x => x.SessionResults);
                var currentCountedResults = currentResults.Take(config.WeeksCounted);
                var currentCountedSessionResults = currentCountedResults.SelectMany(x => x.SessionResults);
                standingRow = AccumulateOverallSessionResults(standingRow, currentMemberSessionResults);
                standingRow = AccumulateCountedSessionResults(standingRow, currentCountedSessionResults);
            }
            else
            {
                standingRow = previousStandingRow;
            }

            memberStandingRows.Add((memberId, previousStandingRow, standingRow));
        }

        // Sort standings
        memberStandingRows = memberStandingRows
            .OrderByDescending(x => x.Current.TotalPoints)
            .ThenByDescending(x => x.Current.PenaltyPoints)
            .ThenByDescending(x => x.Current.Wins)
            .ThenBy(x => x.Current.Incidents)
            .ToList();

        var finalStandingRows = new List<StandingRowCalculationResult>();
        foreach (var (memberStandingRow, position) in memberStandingRows.Select((x, i) => (x, i + 1)))
        {
            memberStandingRow.Current.Position = position;
            var final = DiffStandingRows(memberStandingRow.Previous, memberStandingRow.Current);
            finalStandingRows.Add(final);
        }

        var standingResult = new StandingCalculationResult()
        {
            LeagueId = config.LeagueId,
            EventId = config.EventId,
            Name = config.Name,
            SeasonId = config.SeasonId,
            StandingRows = finalStandingRows
        };
        return Task.FromResult(standingResult);
    }

    private static IComparable GetEventOrderValue(TeamEventResult eventResult)
        => eventResult.SessionResults.Sum(result => result.ResultRow.RacePoints);

    private static StandingRowCalculationResult AccumulateCountedSessionResults(StandingRowCalculationResult standingRow,
        IEnumerable<TeamSessionResultRow> results)
    {
        if (results.None())
        {
            return standingRow;
        }

        standingRow.RacePoints += (int)results.Sum(x => x.ResultRow.RacePoints);
        standingRow.TotalPoints += (int)results.Sum(x => x.ResultRow.TotalPoints);
        standingRow.RacesCounted += results.Count();

        return standingRow;
    }

    private static StandingRowCalculationResult AccumulateOverallSessionResults(StandingRowCalculationResult standingRow,
        IEnumerable<TeamSessionResultRow> results)
    {
        if (results.None())
        {
            return standingRow;
        }

        // accumulate rows
        foreach (var resultRow in results)
        {
            var sessionResult = resultRow.SessionResult;
            var row = resultRow.ResultRow;
            standingRow.CompletedLaps += (int)row.CompletedLaps;
            standingRow.FastestLaps += sessionResult.FastestLapDriverMemberId == standingRow.MemberId ? 1 : 0;
            standingRow.Incidents += (int)row.Incidents;
            standingRow.LeadLaps += (int)row.LeadLaps;
            standingRow.PenaltyPoints += (int)row.PenaltyPoints;
            standingRow.PolePositions += (int)row.StartPosition == 1 ? 1 : 0;
            standingRow.Top10 += row.FinalPosition <= 10 ? 1 : 0;
            standingRow.Top5 += row.FinalPosition <= 5 ? 1 : 0;
            standingRow.Top3 += row.FinalPosition <= 3 ? 1 : 0;
            standingRow.Wins += row.FinalPosition == 1 ? 1 : 0;
            standingRow.Races += 1;
            standingRow.ResultRows.Add(resultRow.ResultRow);
        }

        return standingRow;
    }

    private static StandingRowCalculationResult DiffStandingRows(StandingRowCalculationResult previous, StandingRowCalculationResult current)
    {
        if (previous == current)
        {
            return current;
        }
        var diff = current;

        diff.CompletedLapsChange = current.CompletedLaps - previous.CompletedLaps;
        diff.FastestLapsChange = current.FastestLaps - previous.FastestLaps;
        diff.IncidentsChange = current.Incidents - previous.Incidents;
        diff.LeadLapsChange = current.LeadLaps - previous.LeadLaps;
        diff.PenaltyPointsChange = current.PenaltyPoints - previous.PenaltyPoints;
        diff.PolePositionsChange = current.PolePositions - previous.PolePositions;
        diff.PositionChange = current.Position - previous.Position;
        diff.RacePointsChange = current.RacePoints - previous.RacePoints;
        diff.TotalPointsChange = current.TotalPoints - previous.TotalPoints;
        diff.WinsChange = current.Wins - previous.Wins;

        return diff;
    }

    private record TeamSessionResultRow(long TeamId, SessionCalculationResult SessionResult, ResultRowCalculationResult ResultRow);

    private record TeamEventResult(long TeamId, EventCalculationResult EventResult, IEnumerable<TeamSessionResultRow> SessionResults);
}
