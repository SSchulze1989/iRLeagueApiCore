using FluentValidation;
using iRLeagueApiCore.Communication.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record PostScoringRequest(long LeagueId, long SeasonId, PostScoringModel Model) : IRequest<GetScoringModel>;

    public class PostScoringHandler : IRequestHandler<PostScoringRequest, GetScoringModel>
    {
        private const char pointsDelimiter = ';';

        private readonly ILogger<PostScoringHandler> _logger;
        private readonly LeagueDbContext dbContext;
        private readonly IEnumerable<IValidator<PostScoringRequest>> validators;

        public PostScoringHandler(ILogger<PostScoringHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostScoringRequest>> validators)
        {
            _logger = logger;
            this.dbContext = dbContext;
            this.validators = validators;
        }

        public async Task<GetScoringModel> Handle(PostScoringRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            _logger.LogInformation("Creating scoring for league {LeagueId} in season {SeasonId}",
                request.LeagueId, request.SeasonId);
            var postScoring = await CreateScoringEntityAsync(request.LeagueId, request.SeasonId);
            postScoring = await MapToScoringEntityAsync(request.Model, postScoring);
            dbContext.SaveChanges();
            _logger.LogInformation("Scoring created successfully => scoring id: {ScoringId}", postScoring.ScoringId);
            Debug.Assert(postScoring.ScoringId != default(long));
            var getScoring = await MapToGetScoringModelAsync(postScoring.ScoringId);
            return getScoring;
        }

        private async Task<ScoringEntity> CreateScoringEntityAsync(long leagueId, long seasonId)
        {
            var scoring = new ScoringEntity();
            scoring.LeagueId = leagueId;
            var season = await GetSeasonEntityAsync(seasonId);
            season.Scorings.Add(scoring);
            return scoring;
        }

        private async Task<ScoringEntity> MapToScoringEntityAsync(PostScoringModel source, ScoringEntity target)
        {
            target.AccumulateBy = source.AccumulateBy;
            target.AccumulateResultsOption = source.AccumulateResultsOption;
            target.AverageRaceNr = source.AverageRaceNr;
            target.BasePoints = ConvertBasePoints(source.BasePoints);
            target.BonusPoints = ConvertBonusPoints(source.BonusPoints);
            target.ConnectedSchedule = await GetScheduleEntityAsync(source.ConnectedScheduleId.Value);
            target.Description = source.Description;
            target.ExtScoringSource = await GetScoringEntityAsync(source.ExtScoringSourceId.Value);
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

        private static string ConvertBasePoints(IEnumerable<double> points)
        {
            return string.Join(pointsDelimiter, points.Select(x => x.ToString()));
        }

        private static IEnumerable<double> ConvertBasePoints(string points)
        {
            return points.Split(pointsDelimiter).Select(x => double.Parse(x));
        }

        private static string ConvertBonusPoints(IEnumerable<string> points)
        {
            return string.Join(pointsDelimiter, points);
        }

        private static IEnumerable<string> ConvertBonusPoints(string points)
        {
            return points.Split(pointsDelimiter);
        }

        private async Task<ScheduleEntity> GetScheduleEntityAsync(long scheduleId)
        {
            return await dbContext.Schedules.SingleOrDefaultAsync(x => x.ScheduleId == scheduleId);
        }

        private async Task<ScoringEntity> GetScoringEntityAsync(long scoringId)
        {
            return await dbContext.Scorings.SingleOrDefaultAsync(x => x.ScoringId == scoringId);
        }

        private async Task<SeasonEntity> GetSeasonEntityAsync(long seasonId)
        {
            return await dbContext.Seasons
                .Include(x => x.Scorings)
                .SingleAsync(x => x.SeasonId == seasonId);
        }

        private async Task<GetScoringModel> MapToGetScoringModelAsync(long scoringId)
        {
            return await dbContext.Scorings
                .Where(x => x.ScoringId == scoringId)
                .Select(source => new GetScoringModel()
                {
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
                    ShowResults = source.ShowResults,
                    TakeGroupAverage = source.TakeGroupAverage,
                    TakeResultsFromExtSource = source.TakeResultsFromExtSource,
                    UpdateTeamOnRecalculation = source.UpdateTeamOnRecalculation,
                    UseResultSetTeam = source.UseResultSetTeam,
                })
                .SingleAsync();
        }
    }
}
