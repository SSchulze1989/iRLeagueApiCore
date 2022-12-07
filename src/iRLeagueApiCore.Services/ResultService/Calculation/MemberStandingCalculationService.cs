using iRLeagueApiCore.Services.ResultService.Extensions;
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

        Func<ResultRowCalculationResult, long?> keySelector = x => x.MemberId;

        // flatten results so that we get one entry for each single result row from previous events
        var previousResultRows = previousSessionResults
            .SelectMany(result => result.sessionResults
                .SelectMany(sessionResult => sessionResult.ResultRows
                    .Select(resultRow => (result.eventResult, sessionResult, resultRow))));

        // get the previous result rows for each individual driver
        var previousMemberResultRows = previousResultRows
            .Where(x => keySelector(x.resultRow) is not null)
            .GroupBy(x => keySelector(x.resultRow)!.Value);

        // get the previous result rows per event
        var previousMemberEventResults = previousMemberResultRows
            .Select(x => (key: x.Key, results: x
                .GroupBy(result => result.eventResult, result => new MemberSessionResultRow(x.Key, result.sessionResult, result.resultRow))
                .Select(result => new MemberEventResult(x.Key, result.Key, result))))
            .ToDictionary(k => k.key, v => v.results);

        // do the same for current event result
        var currentResultRows = currentSessionResults.sessionResults
            .SelectMany(sessionResult => sessionResult.ResultRows
                    .Select(resultRow => (sessionResult, resultRow)));
        var currentMemberResultRows = currentResultRows
            .Where(x => x.resultRow.MemberId != null)
            .GroupBy(x => x.resultRow.MemberId!.Value);
        var currentMemberEventResult = currentMemberResultRows.Select(x => (key: x.Key, eventResult: currentSessionResults.eventResult, 
                sessionResults: x.Select(sessionResult => new MemberSessionResultRow(x.Key, sessionResult.sessionResult, sessionResult.resultRow))))
            .ToDictionary(k => k.key, v => new MemberEventResult(v.key, v.eventResult, v.sessionResults));

        var memberIds = previousMemberEventResults.Keys.Concat(currentMemberEventResult.Keys).Distinct();

        List<(long memberId, StandingRowCalculationResult previous, StandingRowCalculationResult current)> memberStandingRows = new();
        foreach (var memberId in memberIds)
        {
            // sort by best race points each event 
            var previousEventResults = (previousMemberEventResults.GetValueOrDefault(memberId) ?? Array.Empty<MemberEventResult>())
                .OrderByDescending(GetEventOrderValue);
            var currentResult = currentMemberEventResult.GetValueOrDefault(memberId);
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
            previousStandingRow = AccumulateTotalPoints(previousStandingRow);

            if (currentResult is not null)
            {
                var currentResults = previousEventResults.Concat(new[] { currentResult })
                    .OrderByDescending(GetEventOrderValue);
                var currentMemberSessionResults = currentResults.SelectMany(x => x.SessionResults);
                var currentCountedResults = currentResults.Take(config.WeeksCounted);
                var currentCountedSessionResults = currentCountedResults.SelectMany(x => x.SessionResults);
                standingRow = AccumulateOverallSessionResults(standingRow, currentMemberSessionResults);
                standingRow = AccumulateCountedSessionResults(standingRow, currentCountedSessionResults);
                standingRow = AccumulateTotalPoints(standingRow);
            }
            else
            {
                standingRow = previousStandingRow;
            }

            memberStandingRows.Add((memberId, previousStandingRow, standingRow));
        }

        // Sort and apply positions standings previous
        memberStandingRows = SortStandingRows(memberStandingRows, x => x.previous)
            .ToList();
        foreach (var (memberStandingRow, position) in memberStandingRows.Select((x, i) => (x, i + 1)))
        {
            memberStandingRow.previous.Position = position;
        }

        // Sort and apply positions standings current
        memberStandingRows = SortStandingRows(memberStandingRows, x => x.current)
            .ToList();

        var finalStandingRows = new List<StandingRowCalculationResult>();
        foreach(var (memberStandingRow, position) in memberStandingRows.Select((x, i) => (x, i + 1)))
        {
            memberStandingRow.current.Position = position;
            var final = DiffStandingRows(memberStandingRow.previous, memberStandingRow.current);
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

    private StandingRowCalculationResult AccumulateTotalPoints(StandingRowCalculationResult row)
    {
        row.TotalPoints = row.RacePoints - row.PenaltyPoints;
        return row;
    }

    private static IComparable GetEventOrderValue(MemberEventResult eventResult)
        => eventResult.SessionResults.Sum(result => result.ResultRow.RacePoints);

    private static StandingRowCalculationResult AccumulateCountedSessionResults(StandingRowCalculationResult standingRow,
        IEnumerable<MemberSessionResultRow> results)
    {
        if (results.None())
        {
            return standingRow;
        }

        standingRow.RacePoints += (int)results.Sum(x => x.ResultRow.RacePoints + x.ResultRow.BonusPoints);
        standingRow.RacesCounted += results.Count();

        return standingRow;
    }

    private static StandingRowCalculationResult AccumulateOverallSessionResults(StandingRowCalculationResult standingRow,
        IEnumerable<MemberSessionResultRow> results)
    {
        if (results.None())
        {
            return standingRow;
        }

        // accumulate rows
        foreach(var resultRow in results)
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

    private static IOrderedEnumerable<T> SortStandingRows<T>(IEnumerable<T> rows, Func<T, StandingRowCalculationResult> standingRowSelector)
    {
        return rows
            .OrderByDescending(x => standingRowSelector(x).TotalPoints)
            .ThenByDescending(x => standingRowSelector(x).PenaltyPoints)
            .ThenByDescending(x => standingRowSelector(x).Wins)
            .ThenBy(x => standingRowSelector(x).Incidents);
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
        diff.PositionChange = -(current.Position - previous.Position);
        diff.RacePointsChange = current.RacePoints - previous.RacePoints;
        diff.TotalPointsChange = current.TotalPoints - previous.TotalPoints;
        diff.WinsChange = current.Wins - previous.Wins;

        return diff;
    }

    private record MemberSessionResultRow(long MemberId, SessionCalculationResult SessionResult, ResultRowCalculationResult ResultRow);

    private record MemberEventResult(long MemberId, EventCalculationResult EventResult, IEnumerable<MemberSessionResultRow> SessionResults);
}
