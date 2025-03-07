﻿using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Standings;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public abstract class StandingsHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    public StandingsHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    /// <summary>
    /// Align the result rows of each standing row so that the sequence in the collection is 
    /// correlating with the event calendar.
    /// That means that for each event in the calendar an entry in the resultRows collection will be created but it will be
    /// null if the driver did not participate in this event.
    /// </summary>
    /// <param name="seasonId"></param>
    /// <param name="standings"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    protected async Task<IEnumerable<StandingsModel>> AlignStandingResultRows(long seasonId, IEnumerable<StandingsModel> standings,
        CancellationToken cancellationToken)
    {
        var events = await dbContext.Events
            .Where(x => x.Schedule.SeasonId == seasonId)
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);
        foreach (var standingRow in standings.SelectMany(x => x.StandingRows))
        {
            standingRow.ResultRows = events.Select(x => standingRow.ResultRows.FirstOrDefault(y => y?.EventId == x.EventId)).ToList();
        }
        return standings;
    }

    protected async Task<DropweekOverrideModel?> MapToDropweekOverrideModel(long standingConfigId, long scoredResultRowId, CancellationToken cancellationToken)
    {
        var dropweek = await dbContext.DropweekOverrides
            .Where(x => x.StandingConfigId == standingConfigId)
            .Where(x => x.ScoredResultRowId == scoredResultRowId)
            .Select(MapToDropweekOverrideModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
        return dropweek;
    }

    protected Expression<Func<StandingEntity, StandingsModel>> MapToStandingModelExpression => standing => new StandingsModel()
    {
        LeagueId = standing.LeagueId,
        SeasonId = standing.SeasonId,
        Name = standing.Name,
        IsTeamStanding = standing.IsTeamStanding,
        StandingId = standing.StandingId,
        StandingRows = standing.StandingRows
            .OrderBy(x => x.Position)
            .Select(standingRow => new StandingRowModel()
            {
                CarClass = standingRow.CarClass,
                ClassId = standingRow.ClassId,
                CompletedLaps = standingRow.CompletedLaps,
                CompletedLapsChange = standingRow.CompletedLapsChange,
                DroppedResultCount = standingRow.DroppedResultCount,
                FastestLapsChange = standingRow.FastestLapsChange,
                FastestLaps = standingRow.FastestLaps,
                Firstname = standingRow.Member == null ? string.Empty : standingRow.Member.Firstname,
                Incidents = standingRow.Incidents,
                IncidentsChange = standingRow.IncidentsChange,
                MemberId = standingRow.MemberId,
                StandingRowId = standingRow.StandingRowId,
                Lastname = standingRow.Member == null ? string.Empty : standingRow.Member.Lastname,
                LastIrating = standingRow.LastIrating,
                LastPosition = standingRow.LastPosition,
                LeadLaps = standingRow.LeadLaps,
                LeadLapsChange = standingRow.LeadLapsChange,
                PenaltyPoints = standingRow.PenaltyPoints,
                PenaltyPointsChange = standingRow.PenaltyPointsChange,
                PolePositions = standingRow.PolePositions,
                PolePositionsChange = standingRow.PolePositionsChange,
                Position = standingRow.Position,
                PositionChange = standingRow.PositionChange,
                RacePoints = standingRow.RacePoints,
                RacePointsChange = standingRow.RacePointsChange,
                Races = standingRow.Races,
                RacesCounted = standingRow.RacesCounted,
                RacesScored = standingRow.RacesScored,
                RacesInPoints = standingRow.RacesInPoints,
                StartIrating = standingRow.StartIrating,
                TeamColor = standingRow.Team == null ? string.Empty : standingRow.Team.TeamColor,
                TeamId = standingRow.TeamId,
                TeamName = standingRow.Team == null ? string.Empty : standingRow.Team.Name,
                Top10 = standingRow.Top10,
                Top3 = standingRow.Top3,
                Top5 = standingRow.Top5,
                TotalPoints = standingRow.TotalPoints,
                TotalPointsChange = standingRow.TotalPointsChange,
                Wins = standingRow.Wins,
                WinsChange = standingRow.WinsChange,
                ResultRows = standingRow.ResultRows.Select(standingResultRow => new StandingResultRowModel()
                {
                    EventId = standingResultRow.ScoredResultRow.ScoredSessionResult.ScoredEventResult.EventId,
                    ResultRowId = standingResultRow.ScoredResultRow.ScoredResultRowId,
                    Date = standingResultRow.ScoredResultRow.ScoredSessionResult.ScoredEventResult.Event.Date != null ?
                            standingResultRow.ScoredResultRow.ScoredSessionResult.ScoredEventResult.Event.Date!.Value : DateTime.MinValue,
                    BonusPoints = standingResultRow.ScoredResultRow.BonusPoints,
                    CompletedLaps = standingResultRow.ScoredResultRow.CompletedLaps,
                    FinalPosition = standingResultRow.ScoredResultRow.FinalPosition,
                    FinishPosition = standingResultRow.ScoredResultRow.FinishPosition,
                    Incidents = standingResultRow.ScoredResultRow.Incidents,
                    Irating = standingResultRow.ScoredResultRow.OldIRating,
                    PenaltyPoints = standingResultRow.ScoredResultRow.PenaltyPoints,
                    RacePoints = standingResultRow.ScoredResultRow.RacePoints,
                    SeasonStartIrating = standingResultRow.ScoredResultRow.SeasonStartIRating,
                    StartPosition = standingResultRow.ScoredResultRow.StartPosition,
                    Status = standingResultRow.ScoredResultRow.Status,
                    TotalPoints = standingResultRow.ScoredResultRow.TotalPoints,
                    IsScored = standingResultRow.IsScored,
                }).ToList(),
                CountryCode = standingRow.CountryCode,
            }).ToList(),
    };

    protected Expression<Func<DropweekOverrideEntity, DropweekOverrideModel>> MapToDropweekOverrideModelExpression => dropweek => new()
    {
        StandingConfigId = dropweek.StandingConfigId,
        ScoredResultRowId = dropweek.ScoredResultRowId,
        Member = dropweek.ScoredResultRow.Member == null ? null : new()
        {
            MemberId = dropweek.ScoredResultRow.Member.Id,
            FirstName = dropweek.ScoredResultRow.Member.Firstname,
            LastName = dropweek.ScoredResultRow.Member.Lastname,
        },
        Team = dropweek.ScoredResultRow.Team == null ? null : new()
        {
            TeamId = dropweek.ScoredResultRow.Team.TeamId,
            Name = dropweek.ScoredResultRow.Team.Name,
            TeamColor = dropweek.ScoredResultRow.Team.TeamColor,
        },
        Date = dropweek.ScoredResultRow.ScoredSessionResult.ScoredEventResult.Event.Date.GetValueOrDefault(),
        TrackId = dropweek.ScoredResultRow.ScoredSessionResult.ScoredEventResult.Event.Track.TrackId,
        TrackName = dropweek.ScoredResultRow.ScoredSessionResult.ScoredEventResult.Event.Track.TrackGroup.TrackName,
        ConfigName = dropweek.ScoredResultRow.ScoredSessionResult.ScoredEventResult.Event.Track.ConfigName,
        ShouldDrop = dropweek.ShouldDrop,
    };
}
