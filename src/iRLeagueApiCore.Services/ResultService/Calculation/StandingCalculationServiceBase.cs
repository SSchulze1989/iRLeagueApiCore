﻿using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using System.Collections;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

internal abstract class StandingCalculationServiceBase : ICalculationService<StandingCalculationData, StandingCalculationResult>
{
    protected readonly StandingCalculationConfiguration config;

    public StandingCalculationServiceBase(StandingCalculationConfiguration config)
    {
        this.config = config;
    }

    protected EventCalculationResult CalculateFinalEventScore<T>(EventCalculationResult data, Func<ResultRowCalculationResult, T?> groupBy)
        where T : struct
    {
        IEqualityComparer groupByComparer = EqualityComparer<T>.Default;
        var practiceSession = data.SessionResults.FirstOrDefault(x => x.ResultRows.FirstOrDefault()?.SessionType == SessionType.Practice);
        var qualySession = data.SessionResults.FirstOrDefault(x => x.ResultRows.FirstOrDefault()?.SessionType == SessionType.Qualifying);
        var raceSessions = data.SessionResults.Where(x => x.ResultRows.FirstOrDefault()?.SessionType == SessionType.Race).ToList();
        if (raceSessions.None())
        {
            return data;
        }

        // if event already has a combined result skip this step
        if (raceSessions.Any(x => x.SessionNr == 999))
        {
            return data;
        }

        // calculate combined result if required
        if (config.UseCombinedResult)
        {
            var allSessionResultRows = data.SessionResults
                .OrderByDescending(x => x.SessionNr)
                .SelectMany(x => x.ResultRows);
            var combinedRows = CalculationServiceBase.CombineResults(allSessionResultRows, groupBy);

            var combinedResult = new SessionCalculationResult()
            {
                LeagueId = config.LeagueId,
                SessionId = null,
                Name = "Temp_Combined",
                SessionResultId = null,
                ResultRows = combinedRows,
                SessionNr = 999,
            };

            (combinedResult.FastestLapDriverMemberId, combinedResult.FastestLap) = data.SessionResults
                .Where(x => x.FastestLapDriverMemberId != null)
                .OrderBy(x => x.FastestLap)
                .Select(x => (x.FastestLapDriverMemberId, x.FastestLap))
                .FirstOrDefault();
            (combinedResult.FastestAvgLapDriverMemberId, combinedResult.FastestAvgLap) = data.SessionResults
                .Where(x => x.FastestAvgLapDriverMemberId != null)
                .OrderBy(x => x.FastestAvgLap)
                .Select(x => (x.FastestAvgLapDriverMemberId, x.FastestAvgLap))
                .FirstOrDefault();
            (combinedResult.FastestQualyLapDriverMemberId, combinedResult.FastestQualyLap) = data.SessionResults
                .Where(x => x.FastestQualyLapDriverMemberId != null)
                .OrderBy(x => x.FastestQualyLap)
                .Select(x => (x.FastestQualyLapDriverMemberId, x.FastestQualyLap))
                .FirstOrDefault();

            data.SessionResults = [.. data.SessionResults, combinedResult];
            return data;
        }

        // if each individual session should be scored add qualy and race points to first race
        var firstRaceSession = raceSessions.First();
        if (practiceSession != null && practiceSession.ResultRows.Any(x => x.RacePoints != 0 || x.BonusPoints != 0 || x.PenaltyPoints != 0))
        {
            foreach (var row in practiceSession.ResultRows)
            {
                var raceRow = firstRaceSession.ResultRows.FirstOrDefault(x => groupByComparer.Equals(groupBy(x), groupBy(row)));
                if (raceRow is null)
                {
                    continue;
                }
                raceRow.RacePoints += row.RacePoints;
                raceRow.BonusPoints += row.BonusPoints;
                raceRow.PenaltyPoints += row.PenaltyPoints;
            }
        }
        if (qualySession != null && qualySession.ResultRows.Any(x => x.RacePoints != 0 || x.BonusPoints != 0 || x.PenaltyPoints != 0))
        {
            foreach (var row in qualySession.ResultRows)
            {
                var raceRow = firstRaceSession.ResultRows.FirstOrDefault(x => groupByComparer.Equals(groupBy(x), groupBy(row)));
                if (raceRow is null)
                {
                    continue;
                }
                raceRow.RacePoints += row.RacePoints;
                raceRow.BonusPoints += row.BonusPoints;
                raceRow.PenaltyPoints += row.PenaltyPoints;
            }
        }

        return data;
    }

    protected (IEnumerable<EventSessionResults> PreviousResults, EventSessionResults CurrentResult) GetPreviousAndCurrentSessionResults(
        StandingCalculationData data)
    {
        IEnumerable<EventSessionResults> previousResults;
        EventSessionResults currentResult;
        if (config.UseCombinedResult)
        {
            previousResults = data.PreviousEventResults
                .Select(eventResult => new EventSessionResults(eventResult, eventResult.SessionResults.OrderBy(x => x.SessionNr).TakeLast(1)));
            currentResult = new EventSessionResults(data.CurrentEventResult, data.CurrentEventResult.SessionResults.OrderBy(x => x.SessionNr).TakeLast(1));
        }
        else
        {
            previousResults = data.PreviousEventResults
                .Select(eventResult => new EventSessionResults(eventResult, eventResult.SessionResults.Where(x => x.SessionType is not (SessionType.Practice or SessionType.Qualifying)
                    && x.SessionNr != 999)));
            currentResult = new EventSessionResults(data.CurrentEventResult,
                data.CurrentEventResult.SessionResults.Where(x => x.SessionType is not (SessionType.Practice or SessionType.Qualifying) && x.SessionNr != 999));
        }
        return (previousResults, currentResult);
    }

    protected static Dictionary<T, GroupedEventResult<T>> GetGroupedEventResult<T>(EventSessionResults sessionResults,
        Func<ResultRowCalculationResult, T?> groupBy) where T : struct
    {
        return GetGroupedEventResults([sessionResults], groupBy)
            .ToDictionary(k => k.Key, v => v.Value.First());
    }

    protected static Dictionary<T, IEnumerable<GroupedEventResult<T>>> GetGroupedEventResults<T>(IEnumerable<EventSessionResults> sessionResults,
        Func<ResultRowCalculationResult, T?> groupBy) where T : struct
    {
        var resultRows = sessionResults
            .SelectMany(result => result.SessionResults
                .SelectMany(sessionResult => sessionResult.ResultRows
                    .Select(resultRow => (result.EventResult, sessionResult, resultRow))));

        // get the previous result rows for each individual driver
        var groupedResultRows = resultRows
            .Where(x => groupBy(x.resultRow) is not null)
            .GroupBy(x => groupBy(x.resultRow)!.Value);

        // get the previous result rows per event
        var groupedEventResults = groupedResultRows
            .Select(x => (key: x.Key, results: x
                .GroupBy(result => result.EventResult, result => new GroupedSessionResultRow<T>(x.Key, result.sessionResult, result.resultRow))
                .Select(result => new GroupedEventResult<T>(x.Key, result.Key, result))))
            .ToDictionary(k => k.key, v => v.results);

        return groupedEventResults;
    }

    public abstract Task<StandingCalculationResult> Calculate(StandingCalculationData data);

    protected static IComparable GetEventOrderValue<T>(GroupedEventResult<T> eventResult) where T : notnull
        => eventResult.SessionResults.Sum(result => result.ResultRow.RacePoints + result.ResultRow.BonusPoints);

    protected static StandingRowCalculationResult AccumulateTotalPoints(StandingRowCalculationResult row)
    {
        row.TotalPoints = row.RacePoints - row.PenaltyPoints;
        return row;
    }

    protected static StandingRowCalculationResult AccumulateCountedSessionResults(StandingRowCalculationResult standingRow,
        IEnumerable<GroupedSessionResultRow<long>> results)
    {
        if (results.None())
        {
            return standingRow;
        }

        standingRow.RacePoints += (int)results.Sum(x => x.ResultRow.RacePoints + x.ResultRow.BonusPoints);
        standingRow.RacesCounted += results.Count();

        return standingRow;
    }

    protected static StandingRowCalculationResult AccumulateOverallSessionResults(StandingRowCalculationResult standingRow,
        IEnumerable<GroupedSessionResultRow<long>> results)
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
            standingRow.RacesScored += row.PointsEligible ? 1 : 0;
            standingRow.RacesInPoints += row.RacePoints > 0 ? 1 : 0;
        }

        return standingRow;
    }

    protected static StandingRowCalculationResult SetScoredResultRows(StandingRowCalculationResult standingRow, IEnumerable<GroupedSessionResultRow<long>> allResults,
        IEnumerable<GroupedSessionResultRow<long>> countedResults)
    {
        standingRow.ResultRows.Clear();
        foreach (var resultRow in allResults)
        {
            var standingResultRow = resultRow.ResultRow;
            standingResultRow.IsScored = countedResults.Contains(resultRow);
            standingRow.ResultRows.Add(standingResultRow);
        }
        return standingRow;
    }

    protected static IEnumerable<T> SortStandingRows<T>(IEnumerable<T> rows, Func<T, StandingRowCalculationResult> standingRowSelector, IEnumerable<SortOptions> sortOptions)
    {
        foreach (var sortOption in sortOptions.Reverse())
        {
            rows = rows.OrderBy(x => sortOption.GetStandingSortingValue<StandingRowCalculationResult>()(standingRowSelector(x)));
        }
        return rows;
    }

    protected static StandingRowCalculationResult DiffStandingRows(StandingRowCalculationResult previous, StandingRowCalculationResult current)
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
        diff.RacePointsChange = current.RacePoints - previous.RacePoints;
        diff.TotalPointsChange = current.TotalPoints - previous.TotalPoints;
        diff.WinsChange = current.Wins - previous.Wins;

        return diff;
    }

    protected static int GetDropweekOverrideOrderValue(GroupedEventResult<long> eventResult)
    {
        var dropweekOverrides = eventResult.SessionResults
            .Select(x => x.ResultRow.DropweekOverride)
            .NotNull()
            .ToList();
        if (dropweekOverrides.Count == 0)
        {
            return 0;
        }
        var shouldDrop = dropweekOverrides.Any(x => x.ShouldDrop);
        return shouldDrop ? 1 : -1; // 1 place at end; -1 place at beginning
    }

    protected record GroupedSessionResultRow<T>(T TeamId, SessionCalculationResult SessionResult, ResultRowCalculationResult ResultRow);

    protected record GroupedEventResult<T>(T TeamId, EventCalculationResult EventResult, IEnumerable<GroupedSessionResultRow<T>> SessionResults);

    protected record EventSessionResults(EventCalculationResult EventResult, IEnumerable<SessionCalculationResult> SessionResults);
}
