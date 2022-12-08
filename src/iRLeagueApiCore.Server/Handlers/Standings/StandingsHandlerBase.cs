using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Standings
{
    public class StandingsHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        public StandingsHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<StandingEntity?> GetStandingEntity(long leagueId, long standingId, CancellationToken cancellationToken)
        {
            return await dbContext.Standings
                .Include(x => x.StandingRows)
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.StandingId == standingId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<StandingsModel?> MapToStandingModel(long leagueId, long standingId, CancellationToken cancellationToken)
        {
            return await dbContext.Standings
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.StandingId == standingId)
                .Select(MapToStandingModelExpression)
                .FirstOrDefaultAsync(cancellationToken);
        }

        protected Expression<Func<StandingEntity, StandingsModel>> MapToStandingModelExpression => standing => new StandingsModel()
        {
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
                ResultRows = Array.Empty<ResultRowModel>(),
            })
        };
    }
}
