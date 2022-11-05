using iRLeagueApiCore.Services.ResultService.Extensions;
using iRLeagueApiCore.Services.ResultService.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Services.ResultService.DataAccess
{
    internal sealed class EventResultCalculationResultStore : DatabaseAccessBase, IEventResultCalculationResultStore
    {
        public EventResultCalculationResultStore(ILeagueDbContext dbContext) : 
            base(dbContext)
        {
        }

        public async Task StoreCalculationResult(EventResultCalculationResult result, CancellationToken cancellationToken = default)
        {
            var eventResultEntity = await GetScoredEventResultEntity(result.EventId, result.ResultConfigId, cancellationToken)
                ?? await CreateScoredResultEntity(result.EventId, result.ResultConfigId, cancellationToken);
            var requiredEntities = await GetRequiredEntities(result, cancellationToken);
            await MapToEventResultEntity(result, eventResultEntity, requiredEntities, cancellationToken);
            return;
        }

        private async Task<ScoredEventResultEntity> MapToEventResultEntity(EventResultCalculationResult result, ScoredEventResultEntity entity,
            RequiredEntities requiredEntities, CancellationToken cancellationToken)
        {
            var members = await GetMemberEntities(result.SessionResults
                .SelectMany(x => x.ResultRows)
                .Select(x => x.MemberId)
                .OfType<long>(), cancellationToken);
            var teams = await GetTeamEntities(result.SessionResults
                .SelectMany(x => x.ResultRows)
                .Select(x => x.TeamId)
                .OfType<long>(), cancellationToken);
            entity.Name = result.Name;
            entity.ScoredSessionResults = await MapToScoredSessionResults(result.SessionResults, entity.ScoredSessionResults, 
                requiredEntities, cancellationToken);

            return entity;
        }

        private async Task<ICollection<ScoredSessionResultEntity>> MapToScoredSessionResults(IEnumerable<SessionResultCalculationResult> sessionResults, 
            ICollection<ScoredSessionResultEntity> sessionResultEntites, RequiredEntities requiredEntities, CancellationToken cancellationToken)
        {
            var keepResults = new List<ScoredSessionResultEntity>();
            foreach(var sessionResult in sessionResults)
            {
                var sessionResultEntity = sessionResultEntites
                    .FirstOrDefault(x => x.SessionResultId == sessionResult.SessionResultId)
                    ?? await CreateScoredSessionResultEntity(sessionResult.ScoringId, cancellationToken);
                sessionResultEntity = MapToScoredSessionResultEntity(sessionResult, sessionResultEntity, requiredEntities);
            }
            return sessionResultEntites;
        }

        private ScoredSessionResultEntity MapToScoredSessionResultEntity(SessionResultCalculationResult result, ScoredSessionResultEntity entity,
            RequiredEntities requiredEntities)
        {
            entity.CleanestDrivers = requiredEntities.Members.Where(x => result.CleanestDrivers.Contains(x.Id)).ToList();
            entity.FastestAvgLap = result.FastestAvgLap.Ticks;
            entity.FastestAvgLapDriver = requiredEntities.Members.FirstOrDefault(x => x.Id == result.FastestQualyLapDriverMemberId);
            entity.FastestLap = result.FastestLap.Ticks;
            entity.FastestAvgLapDriver = requiredEntities.Members.FirstOrDefault(x => x.Id == result.FastestAvgLapDriverMemberId);
            entity.FastestQualyLap = result.FastestQualyLap.Ticks;
            entity.FastestQualyLapDriver = requiredEntities.Members.FirstOrDefault(x => x.Id == result.FastestQualyLapDriverMemberId);
            entity.HardChargers = requiredEntities.Members.Where(x => result.HardChargers.Contains(x.Id)).ToList();
            entity.ScoredResultRows = MapToScoredResultRows(result.ResultRows, entity.ScoredResultRows, requiredEntities);

            return entity;
        }

        private ICollection<ScoredResultRowEntity> MapToScoredResultRows(IEnumerable<ResultRowCalculationResult> resultRows, 
            ICollection<ScoredResultRowEntity> rowEntities, RequiredEntities requiredEntities)
        {
            foreach(var row in resultRows)
            {
                ScoredResultRowEntity? rowEntity = default;
                if (row.MemberId != null)
                {
                    rowEntity = rowEntities
                        .FirstOrDefault(x => x.MemberId == row.MemberId);
                }
                else if (row.TeamId != null)
                {
                    rowEntity = rowEntities
                        .FirstOrDefault(x => x.TeamId == row.TeamId);
                }
                if (rowEntity == null)
                {
                    rowEntity = new ScoredResultRowEntity();
                    rowEntities.Add(rowEntity);
                }
                rowEntity = MaptoScoredResultRow(row, rowEntity, requiredEntities);
            }
            return rowEntities;
        }

        private ScoredResultRowEntity MaptoScoredResultRow(ResultRowCalculationResult row, ScoredResultRowEntity rowEntity, 
            RequiredEntities requiredEntities)
        {
            rowEntity.Member = requiredEntities.Members.FirstOrDefault(x => x.Id == row.MemberId);
            rowEntity.Team = requiredEntities.Teams.FirstOrDefault(x => x.TeamId == row.TeamId);
            rowEntity.AddPenalty = requiredEntities.AddPenalties.FirstOrDefault(x => x.ScoredResultRowId == rowEntity.ScoredResultRowId);
            rowEntity.AvgLapTime = row.AvgLapTime;
            rowEntity.BonusPoints = row.BonusPoints;
            rowEntity.Car = row.Car;
            rowEntity.CarClass = row.CarClass;
            rowEntity.CarId = row.CarId;
            rowEntity.CarNumber = row.CarNumber;
            rowEntity.ClassId = row.ClassId;
            rowEntity.ClubId = row.ClubId;
            rowEntity.ClubName = row.ClubName;
            rowEntity.CompletedLaps = row.CompletedLaps;
            rowEntity.CompletedPct = row.CompletedPct.GetValueOrDefault();
            rowEntity.FastestLapTime = row.FastestLapTime;
            rowEntity.FastLapNr = row.FastLapNr;
            rowEntity.FinalPosition = row.FinalPosition;
            rowEntity.FinalPositionChange = row.FinalPositionChange;
            rowEntity.FinishPosition = row.FinishPosition;
            rowEntity.Incidents = row.Incidents;
            rowEntity.Interval = row.Interval;
            rowEntity.LeadLaps = row.LeadLaps;
            rowEntity.License = row.License;
            rowEntity.NewCpi = row.NewCpi;
            rowEntity.NewIRating = row.NewIrating;
            rowEntity.NewLicenseLevel = row.NewLicenseLevel;
            rowEntity.NewSafetyRating = row.NewSafetyRating;
            rowEntity.OldCpi = row.OldCpi;
            rowEntity.OldIRating = row.OldIrating;
            rowEntity.OldLicenseLevel = row.OldLicenseLevel;
            rowEntity.OldSafetyRating = row.OldSafetyRating;
            rowEntity.PenaltyPoints = row.PenaltyPoints;
            rowEntity.PositionChange = row.PositionChange;
            rowEntity.QualifyingTime = row.QualifyingTime;
            rowEntity.RacePoints = row.RacePoints;
            // TODO: Get review penaltie entities from calculated penalties
            //rowEntity.ReviewPenalties = requiredEntities.ReviewPenalties
            rowEntity.SeasonStartIRating = row.SeasonStartIrating;
            rowEntity.StartPosition = row.StartPosition;
            rowEntity.Status = row.Status;
            rowEntity.TotalPoints = row.TotalPoints;
            return rowEntity;
        }

        private async Task<ScoredSessionResultEntity> CreateScoredSessionResultEntity(long? scoringId, CancellationToken cancellationToken)
        {
            var scoring = await dbContext.Scorings
                .FirstOrDefaultAsync(x => x.ScoringId == scoringId, cancellationToken);
            var sessionResult = new ScoredSessionResultEntity()
            {
                Scoring = scoring,
            };
            return sessionResult;
        }

        private async Task<ScoredEventResultEntity> CreateScoredResultEntity(long eventId, long? resultConfigId, CancellationToken cancellationToken)
        {
            var @event = await dbContext.Events
                .FirstOrDefaultAsync(x => x.EventId == eventId, cancellationToken)
                ?? throw new InvalidOperationException($"No event with id:{eventId} exists");
            var resultConfig = await dbContext.ResultConfigurations
                .FirstOrDefaultAsync(x => x.ResultConfigId == resultConfigId, cancellationToken);
            var eventResult = new ScoredEventResultEntity()
            {
                Event = @event,
                ResultConfig = resultConfig,
            };
            return eventResult;
        }

        private async Task<ScoredEventResultEntity?> GetScoredEventResultEntity(long eventId, long? resultConfigId, CancellationToken cancellationToken)
        {
            return await dbContext.ScoredEventResults
                .Include(x => x.ScoredSessionResults)
                    .ThenInclude(x => x.ScoredResultRows)
                .Where(x => x.EventId == eventId)
                .Where(x => x.ResultConfigId == resultConfigId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        private async Task<RequiredEntities> GetRequiredEntities(EventResultCalculationResult result, CancellationToken cancellationToken)
        {
            RequiredEntities requiredEntities = new();
            requiredEntities.Members = await GetMemberEntities(result.SessionResults
                .SelectMany(x => x.ResultRows)
                .Select(x => x.MemberId)
                .OfType<long>(), cancellationToken);
            requiredEntities.Teams = await GetTeamEntities(result.SessionResults
                .SelectMany(x => x.ResultRows)
                .Select(x => x.TeamId)
                .OfType<long>(), cancellationToken);
            return requiredEntities;
        }

        private async Task<ICollection<MemberEntity>> GetMemberEntities(IEnumerable<long> memberIds, CancellationToken cancellationToken)
        {
            return await dbContext.Members
                .Where(x => memberIds.Contains(x.Id))
                .ToListAsync(cancellationToken);
        }

        private async Task<ICollection<TeamEntity>> GetTeamEntities(IEnumerable<long> teamIds, CancellationToken cancellationToken)
        {
            return await dbContext.Teams
                .Where(x => teamIds.Contains(x.TeamId))
                .ToListAsync(cancellationToken);
        }
    }
}
