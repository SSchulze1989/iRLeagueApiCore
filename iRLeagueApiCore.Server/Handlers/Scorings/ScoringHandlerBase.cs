using iRLeagueApiCore.Common.Models;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public class ScoringHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
    {
        protected const char pointsDelimiter = ';';

        public ScoringHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<FluentValidation.IValidator<TRequest>> validators) : 
            base(logger, dbContext, validators)
        {
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

        protected virtual async Task<ScoringEntity> MapToScoringEntityAsync(long leagueId, PostScoringModel source, ScoringEntity target, 
            CancellationToken cancellationToken = default)
        {
            target.ExtScoringSource = await GetScoringEntityAsync(leagueId, source.ExtScoringSourceId, cancellationToken);
            target.MaxResultsPerGroup = source.MaxResultsPerGroup;
            target.Name = source.Name;
            target.ShowResults = source.ShowResults;
            target.UpdateTeamOnRecalculation = source.UpdateTeamOnRecalculation;
            target.UseResultSetTeam = source.UseResultSetTeam;
            return target;
        }

        protected virtual async Task<ScoringModel> MapToGetScoringModelAsync(long leagueId, long scoringId, CancellationToken cancellationToken = default)
        {
            return await dbContext.Scorings
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.ScoringId == scoringId)
                .Select(MapToGetScoringModelExpression)
                .SingleOrDefaultAsync(cancellationToken);
        }

        protected Expression<Func<ScoringEntity, ScoringModel>> MapToGetScoringModelExpression => source => new ScoringModel()
        {
            Id = source.ScoringId,
            MaxResultsPerGroup = source.MaxResultsPerGroup,
            Name = source.Name,
            ShowResults = source.ShowResults,
            UpdateTeamOnRecalculation = source.UpdateTeamOnRecalculation,
            UseResultSetTeam = source.UseResultSetTeam,
        };
    }
}
