using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public class ScoringHandlerBase
    {
        protected readonly LeagueDbContext dbContext;
        protected const char pointsDelimiter = ';';

        public ScoringHandlerBase(LeagueDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        protected static string ConvertBasePoints(IEnumerable<double> points)
        {
            return string.Join(pointsDelimiter, points.Select(x => x.ToString()));
        }

        protected static IEnumerable<double> ConvertBasePoints(string points)
        {
            return points?.Split(pointsDelimiter)
                .Where(x => string.IsNullOrEmpty(x) == false)
                .Select(x => double.Parse(x)) ?? new double[0];
        }

        protected static string ConvertBonusPoints(IEnumerable<string> points)
        {
            return string.Join(pointsDelimiter, points);
        }

        protected static IEnumerable<string> ConvertBonusPoints(string points)
        {
            return points?
                .Split(pointsDelimiter)
                .Where(x => string.IsNullOrEmpty(x) == false) ?? new string[0];
        }

        protected virtual async Task<ScheduleEntity> GetScheduleEntityAsync(long leagueId, long? scheduleId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Schedules
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.ScheduleId == scheduleId, cancellationToken);
        }

        protected virtual async Task<ScoringEntity> GetScoringEntityAsync(long leagueId, long? scoringId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.ScoringId == scoringId, cancellationToken);
        }

        protected virtual async Task<SeasonEntity> GetSeasonEntityAsync(long leagueId, long seasonId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Seasons
                .Include(x => x.Scorings)
                .Where(x => x.LeagueId == leagueId)
                .SingleOrDefaultAsync(x => x.SeasonId == seasonId, cancellationToken);
        }

        protected virtual async Task<ScoringEntity> MapToScoringEntityAsync(long leagueId, PostScoringModel source, ScoringEntity target, 
            CancellationToken cancellationToken = default)
        {
            target.AccumulateBy = source.AccumulateBy;
            target.AccumulateResultsOption = source.AccumulateResultsOption;
            target.AverageRaceNr = source.AverageRaceNr;
            target.BasePoints = ConvertBasePoints(source.BasePoints);
            target.BonusPoints = ConvertBonusPoints(source.BonusPoints);
            target.ConnectedSchedule = await GetScheduleEntityAsync(leagueId, source.ConnectedScheduleId, cancellationToken);
            target.Description = source.Description;
            target.ExtScoringSource = await GetScoringEntityAsync(leagueId, source.ExtScoringSourceId, cancellationToken);
            target.FinalSortOptions = source.FinalSortOptions;
            target.MaxResultsPerGroup = source.MaxResultsPerGroup;
            target.Name = source.Name;
            target.PointsSortOptions = source.PointsSortOptions;
            target.ScoringKind = (int)source.ScoringKind;
            target.ScoringSessionType = source.ScoringSessionType;
            target.ScoringWeightValues = source.ScoringWeightValues;
            target.SessionSelectType = source.SessionSelectType;
            target.ShowResults = source.ShowResults;
            target.TakeGroupAverage = source.TakeGroupAverage;
            target.TakeResultsFromExtSource = source.TakeResultsFromExtSource;
            target.UpdateTeamOnRecalculation = source.UpdateTeamOnRecalculation;
            target.UseResultSetTeam = source.UseResultSetTeam;
            return target;
        }

        protected virtual async Task<GetScoringModel> MapToGetScoringModelAsync(long leagueId, long scoringId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ScoringId == scoringId)
                .Select(MapToGetScoringModelExpression)
                .SingleOrDefaultAsync(cancellationToken);
        }

        protected Expression<Func<ScoringEntity, GetScoringModel>> MapToGetScoringModelExpression => source => new GetScoringModel()
        {
            Id = source.ScoringId,
            AccumulateBy = source.AccumulateBy,
            AccumulateResultsOption = source.AccumulateResultsOption,
            AverageRaceNr = source.AverageRaceNr,
            BasePoints = ConvertBasePoints(source.BasePoints),
            BonusPoints = ConvertBonusPoints(source.BonusPoints),
            ConnectedScheduleId = source.ConnectedScheduleId,
            Description = source.Description,
            ExtScoringSourceId = source.ExtScoringSourceId,
            FinalSortOptions = source.FinalSortOptions,
            MaxResultsPerGroup = source.MaxResultsPerGroup,
            Name = source.Name,
            PointsSortOptions = source.PointsSortOptions,
            ScoringSessionType = source.ScoringSessionType,
            ScoringWeightValues = source.ScoringWeightValues,
            SessionSelectType = source.SessionSelectType,
            SessionIds = source.Sessions.Select(x => x.SessionId),
            ShowResults = source.ShowResults,
            TakeGroupAverage = source.TakeGroupAverage,
            TakeResultsFromExtSource = source.TakeResultsFromExtSource,
            UpdateTeamOnRecalculation = source.UpdateTeamOnRecalculation,
            UseResultSetTeam = source.UseResultSetTeam,
        };
    }
}
