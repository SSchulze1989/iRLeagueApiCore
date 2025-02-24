using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;

abstract internal class CalculationServiceBase : ICalculationService<SessionCalculationData, SessionCalculationResult>
{
    public abstract Task<SessionCalculationResult> Calculate(SessionCalculationData data);

    protected static IEnumerable<ResultRowCalculationResult> ApplyPoints(IEnumerable<ResultRowCalculationResult> rows, PointRule<ResultRowCalculationResult> pointRule,
        SessionCalculationData data)
    {
        rows = pointRule.GetChampSeasonFilters().FilterRows(rows);
        rows = pointRule.GetResultFilters().FilterRows(rows);
        CalculateCompletedPct(rows);
        CalculateIntervals(rows);
        CalculateAutoPenalties(rows, pointRule.GetAutoPenalties());
        AddReviewPenalties(rows, data.AcceptedReviewVotes);
        ApplyAddPenaltyDsq(rows);
        ApplyAddPenaltyTimes(rows);
        rows = pointRule.SortForPoints(rows);
        rows = ApplyAddPenaltyPositions(rows);

        IEnumerable<ResultRowCalculationResult> pointRows = rows.ToList();
        // Filter for points only
        pointRows = pointRule.GetPointFilters().FilterRows(pointRows);

        // Calculation
        pointRule.ApplyPoints(data, pointRows.ToList());
        // remove points from filtered rows and set points eligible 
        pointRows.ForEach(x => x.PointsEligible = true);
        rows.Except(pointRows)
            .ForEach(x =>
            {
                x.RacePoints = 0;
                x.PointsEligible = false;
            });

        IEnumerable<ResultRowCalculationResult> finalRows = rows;
        ApplyAddPenaltyPoints(finalRows);
        ApplyBonusPoints(pointRows, pointRule.GetBonusPoints());
        ApplyTotalPoints(finalRows);
        finalRows = pointRule.SortFinal(finalRows);
        // Set final position
        foreach ((var row, var position) in finalRows.Select((x, i) => (x, i + 1)))
        {
            row.FinalPosition = position;
            row.FinalPositionChange = row.StartPosition - row.FinalPosition;
        }

        return finalRows;
    }

    protected static (TId? id, TimeSpan lap) GetBestLapValue<T, TId>(IEnumerable<T> rows, Func<T, TId?> idSelector, Func<T, TimeSpan> valueSelector)
    {
        return rows
            .Select(row => ((TId? id, TimeSpan lap))(idSelector.Invoke(row), valueSelector.Invoke(row)))
            .Where(row => LapIsValid(row.lap))
            .DefaultIfEmpty()
            .MinBy(row => row.lap);
    }

    protected static IEnumerable<(TId? id, TValue value)> GetBestValues<T, TValue, TId>(IEnumerable<T> rows, Func<T, TValue> valueSelector, Func<T, TId?> idSelector,
        Func<IEnumerable<TValue>, TValue> bestValueFunc, EqualityComparer<TValue>? comparer = default)
    {
        comparer ??= EqualityComparer<TValue>.Default;
        try
        {
            var valueRows = rows.Select(row => ((TId? id, TValue value))(idSelector.Invoke(row), valueSelector.Invoke(row)));
            var bestValue = bestValueFunc.Invoke(valueRows.Select(x => x.value));
            return valueRows.Where(row => comparer.Equals(row.value, bestValue));
        }
        catch (Exception ex) when (ex is InvalidOperationException)
        {
            return [];
        }
    }

    protected static TimeSpan GetAverageLapValue<T>(IEnumerable<T> rows, Func<T, TimeSpan> valueSelector, Func<T, double> weightSelector)
    {
        double total = 0;
        TimeSpan lap = TimeSpan.Zero;
        foreach (var row in rows)
        {
            var w = weightSelector(row);
            total += w;
            lap += valueSelector(row) * w;
        }

        return lap / (total > 0 ? total : 1);
    }

    protected static bool LapIsValid(TimeSpan lap)
    {
        return lap > TimeSpan.Zero.Add(TimeSpan.FromMilliseconds(1));
    }

    protected static bool HardChargerEligible(ResultRowCalculationResult row)
    {
        return row.QualifyingTime > TimeSpan.Zero;
    }

    /// <summary>
    /// Group and combine result rows using the given groupBy key selector
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="rows">rows to combine</param>
    /// <param name="groupBy">selector for the key by which to group the rows</param>
    /// <returns></returns>
    internal static IEnumerable<ResultRowCalculationResult> CombineResults<T>(IEnumerable<ResultRowCalculationResult> rows, Func<ResultRowCalculationResult, T> groupBy)
    {
        var groupedRows = rows.GroupBy(groupBy);
        var combined = new List<ResultRowCalculationResult>();

        foreach (var group in groupedRows.Where(x => x.Any()))
        {
            var raceSessions = group.Where(x => x.SessionType == SessionType.Race).ToList();
            if (raceSessions.Count == 0)
            {
                continue;
            }
            var row = new ResultRowCalculationResult(raceSessions.First());

            foreach (var sessionRow in raceSessions.Skip(1))
            {
                row.BonusPoints += sessionRow.BonusPoints;
                row.CompletedLaps += sessionRow.CompletedLaps;
                row.Incidents += sessionRow.Incidents;
                row.LeadLaps += sessionRow.LeadLaps;
                row.PenaltyPoints += sessionRow.PenaltyPoints;
                row.RacePoints += sessionRow.RacePoints;
                row.PointsEligible |= sessionRow.PointsEligible;
                row.Status = CombineRaceStatus(row.Status, sessionRow.Status);
            }
            // handle practice and qualy sessions separately
            var otherSessions = group.Except(raceSessions);
            foreach (var sessionRow in otherSessions)
            {
                row.BonusPoints += sessionRow.BonusPoints;
                row.RacePoints += sessionRow.RacePoints;
                row.PenaltyPoints += sessionRow.PenaltyPoints;
            }

            row.StartPosition = raceSessions.Last().StartPosition;
            row.AvgLapTime = GetAverageLapValue(raceSessions, x => x.AvgLapTime, x => x.CompletedLaps);
            (_, row.FastestLapTime) = GetBestLapValue(raceSessions, x => x.MemberId, x => x.FastestLapTime);
            (_, row.QualifyingTime) = GetBestLapValue(raceSessions, x => x.MemberId, x => x.QualifyingTime);
            row.FastLapNr = 0;
            var last = raceSessions.Last();
            row.NewCpi = last.NewCpi;
            row.NewIrating = last.NewIrating;
            row.NewLicenseLevel = last.NewLicenseLevel;
            row.NewSafetyRating = last.NewSafetyRating;
            combined.Add(row);
        }

        return combined.ToList();
    }

    protected static RaceStatus CombineRaceStatus(params RaceStatus[] raceStatuses)
    {
        if (raceStatuses.Length == 0)
        {
            return RaceStatus.Unknown;
        }

        var currentStatus = raceStatuses[0];
        foreach (var raceStatus in raceStatuses.Skip(1))
        {
            if (currentStatus == raceStatus)
            {
                continue;
            }
            if (currentStatus.GetRaceStatusOrderValue() <= raceStatus.GetRaceStatusOrderValue())
            {
                continue;
            }
            currentStatus = raceStatus;
        }
        return currentStatus;
    }

    private static AddPenaltyCalculationData CreateAddPenaltyFromReviewPenalty(ResultRowCalculationResult row, ReviewPenaltyCalculationResult reviewPenalty)
    {
        return new()
        {
            MemberId = row.MemberId,
            TeamId = row.MemberId is null ? row.TeamId : null,
            Type = reviewPenalty.Value.Type,
            Points = reviewPenalty.Value.Type == PenaltyType.Points ? reviewPenalty.Value.Points : 0,
            Positions = reviewPenalty.Value.Type == PenaltyType.Position ? reviewPenalty.Value.Positions : 0,
            Time = reviewPenalty.Value.Type == PenaltyType.Time ? reviewPenalty.Value.Time : TimeSpan.Zero,
        };
    }

    private static AddPenaltyCalculationData CreateAddPenaltyFromAutoPenalty(ResultRowCalculationResult row, AutoPenaltyConfigurationData autoPenalty,
        int penaltyMultiplikator)
    {
        var penalty = new AddPenaltyCalculationData()
        {
            MemberId = row.MemberId,
            TeamId = row.MemberId is null ? row.TeamId : null,
            Points = autoPenalty.Type == PenaltyType.Points ? autoPenalty.Points * penaltyMultiplikator : 0,
            Positions = autoPenalty.Type == PenaltyType.Position ? autoPenalty.Positions * penaltyMultiplikator : 0,
            Time = autoPenalty.Type == PenaltyType.Time ? autoPenalty.Time * penaltyMultiplikator : TimeSpan.Zero,
            Type = autoPenalty.Type,
        };
        return penalty;
    }

    private static IEnumerable<ResultRowCalculationResult> CalculateAutoPenalties(IEnumerable<ResultRowCalculationResult> rows,
        IEnumerable<AutoPenaltyConfigurationData> autoPenalties)
    {
        foreach (var autoPenalty in autoPenalties)
        {
            var penaltyRows = autoPenalty.Conditions
                .FilterRows(rows);
            var grouped = penaltyRows.GroupBy(x => x);
            foreach (var row in grouped.Where(x => x.Any()))
            {
                row.Key.AddPenalties.Add(CreateAddPenaltyFromAutoPenalty(row.Key, autoPenalty, row.Count()));
            }
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyAddPenaltyDsq(IEnumerable<ResultRowCalculationResult> rows)
    {
        foreach (var row in rows.Where(x => x.AddPenalties.Any(x => x.Type == PenaltyType.Disqualification)))
        {
            row.Status = RaceStatus.Disqualified;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyAddPenaltyTimes(IEnumerable<ResultRowCalculationResult> rows)
    {
        foreach (var row in rows)
        {
            foreach (var penalty in row.AddPenalties.Where(x => x.Type == PenaltyType.Time))
            {
                row.Interval += penalty.Time;
                row.PenaltyTime += penalty.Time;
            }
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyAddPenaltyPoints(IEnumerable<ResultRowCalculationResult> rows)
    {
        foreach (var row in rows.Where(x => x.AddPenalties.Any(x => x.Type == PenaltyType.Points)))
        {
            foreach (var penalty in row.AddPenalties.Where(x => x.Type == PenaltyType.Points))
            {
                if (penalty.Points < 0)
                {
                    row.BonusPoints -= penalty.Points;
                }
                else
                {
                    row.PenaltyPoints += penalty.Points;
                }
            }
        }
        return rows;
    }

    private static List<ResultRowCalculationResult> ApplyAddPenaltyPositions(IEnumerable<ResultRowCalculationResult> rows)
    {
        var modRows = rows.ToList();
        foreach (var row in rows.Where(x => x.AddPenalties.Any(x => x.Type == PenaltyType.Position)).Reverse()) // Start from the back to keep order between mutliple drivers with penalties
        {
            var movePositions = row.AddPenalties.Where(x => x.Type == PenaltyType.Position).Sum(x => x.Positions);
            if (movePositions == 0)
            {
                continue;
            }
            row.PenaltyPositions = movePositions;
            var rowIndex = modRows.IndexOf(row);
            modRows.Move(rowIndex, movePositions);
        }
        return modRows;
    }

    private static IEnumerable<ResultRowCalculationResult> AddReviewPenalties(IEnumerable<ResultRowCalculationResult> rows, IEnumerable<AcceptedReviewVoteCalculationData> reviewVotes)
    {
        Func<ResultRowCalculationResult, AcceptedReviewVoteCalculationData, bool> compareIds;
        if (rows.Any(x => x.MemberId != null))
        {
            compareIds = (row, vote) => vote.MemberAtFaultId == row.MemberId;
        }
        else
        {
            compareIds = (row, vote) => vote.TeamAtFaultId == row.TeamId;
        }

        foreach (var row in rows)
        {
            var rowVotes = reviewVotes.Where(vote => compareIds(row, vote));
            foreach (var vote in rowVotes)
            {
                var penalty = new ReviewPenaltyCalculationResult()
                {
                    ReviewId = vote.ReviewId,
                    ReviewVoteId = vote.ReviewVoteId,
                    Value = vote.DefaultPenalty,
                };
                row.ReviewPenalties.Add(penalty);
                row.AddPenalties.Add(CreateAddPenaltyFromReviewPenalty(row, penalty));
            }
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyTotalPoints(IEnumerable<ResultRowCalculationResult> rows)
    {
        foreach (var row in rows)
        {
            row.TotalPoints = row.RacePoints + row.BonusPoints - row.PenaltyPoints;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyBonusPoints(IEnumerable<ResultRowCalculationResult> rows, IEnumerable<BonusPointConfiguration> BonusPoints)
    {
        if (rows.None())
        {
            return rows;
        }

        var minIncs = rows.Any(x => x.PenaltyPoints == 0) ? rows.Where(x => x.PenaltyPoints == 0).Min(x => x.Incidents) : -1;

        foreach (var bonus in BonusPoints)
        {
            var bonusType = bonus.Type;
            var bonusKeyValue = bonus.Value;
            var bonusPoints = bonus.Points;
            rows = bonusType switch
            {
                BonusPointType.Position => ApplyPositionBonusPoints(rows, bonusKeyValue, bonusPoints),
                BonusPointType.CleanestDriver => ApplyCleanestDriverBonusPoints(rows, bonusPoints),
                BonusPointType.FastestLap => ApplyFastestLapBonusPoints(rows, bonusPoints),
                BonusPointType.QualyPosition => ApplyStartPositionBonusPoints(rows, bonusKeyValue, bonusPoints),
                BonusPointType.MostPositionsGained => ApplyMostPositionsGainedBonusPoints(rows, bonusPoints),
                BonusPointType.MostPositionsLost => ApplyMostPositionsLostBonusPoints(rows, bonusPoints),
                BonusPointType.LeadOneLap => ApplyLeadOneLapBonusPoints(rows, bonusPoints),
                BonusPointType.LeadMostLaps => ApplyLeadMostLapsBonusPoints(rows, bonusPoints),
                BonusPointType.NoIncidents => ApplyNoIncidentsBonusPoints(rows, bonusPoints),
                BonusPointType.FastestAverageLap => ApplyFastestAverageLapBonusPoints(rows, bonusPoints),
                BonusPointType.Custom => ApplyCustomBonusPoints(rows, bonus, bonusPoints),
                _ => rows,
            };
        }

        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyCustomBonusPoints(IEnumerable<ResultRowCalculationResult> rows, BonusPointConfiguration bonus,
        int bonusPoints)
    {
        var bonusRows = bonus.Conditions.FilterRows(rows);

        foreach (var row in bonusRows)
        {
            row.BonusPoints += bonusPoints;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyFastestAverageLapBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var (row, lapTime) = GetBestLapValue(rows, x => x, x => x.AvgLapTime);
        if (row is not null)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyNoIncidentsBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var bonusRows = rows.Where(x => x.Incidents == 0);

        foreach (var row in bonusRows)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyLeadMostLapsBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var mostLapsLead = rows.Max(x => x.LeadLaps);
        var bonusRows = rows.Where(x => x.LeadLaps == mostLapsLead);

        foreach (var row in bonusRows)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyLeadOneLapBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var bonusRows = rows.Where(x => x.LeadLaps > 0);

        foreach (var row in bonusRows)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyMostPositionsLostBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var mostPositionsLost = rows.Max(x => x.PositionChange);
        var bonusRows = rows.Where(x => x.PositionChange == mostPositionsLost);

        foreach (var row in bonusRows)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyMostPositionsGainedBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var mostPositionsGained = rows.Min(x => x.PositionChange);
        var bonusRows = rows.Where(x => x.PositionChange == mostPositionsGained);

        foreach (var row in bonusRows)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyPositionBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int position, int points)
    {
        foreach (var row in rows)
        {
            if (row.FinalPosition == position)
            {
                row.BonusPoints += points;
            }
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyCleanestDriverBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        static bool condition(ResultRowCalculationResult x) => x.PenaltyPoints == 0;
        var minIncRows = GetBestValues(rows.Where(condition), x => x.Incidents, x => x, x => x.Min())
            .Select(x => x.id)
            .NotNull();

        foreach (var row in minIncRows)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyFastestLapBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int points)
    {
        var (row, lapTime) = GetBestLapValue(rows, x => x, x => x.FastestLapTime);
        if (row is not null)
        {
            row.BonusPoints += points;
        }
        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> ApplyStartPositionBonusPoints(IEnumerable<ResultRowCalculationResult> rows, int position, int points)
    {
        foreach (var row in rows)
        {
            if (row.StartPosition == position)
            {
                row.BonusPoints += points;
            }
        }
        return rows;
    }

    protected static IEnumerable<ResultRowCalculationResult> CalculateCompletedPct(IEnumerable<ResultRowCalculationResult> rows)
    {
        var laps = rows.MaxOrDefault(x => x.CompletedLaps);
        if (laps == 0)
        {
            return rows;
        }

        foreach (var row in rows)
        {
            row.CompletedPct = row.CompletedLaps / laps;
        }

        return rows;
    }

    private static IEnumerable<ResultRowCalculationResult> CalculateIntervals(IEnumerable<ResultRowCalculationResult> rows)
    {
        int totalLaps = (int)rows.MaxOrDefault(x => x.CompletedLaps);
        foreach (var row in rows)
        {
            if (row.Interval.Days > 0)
            {
                row.Interval = TimeSpan.FromDays(totalLaps - row.CompletedLaps);
            }
        }
        return rows;
    }
}
