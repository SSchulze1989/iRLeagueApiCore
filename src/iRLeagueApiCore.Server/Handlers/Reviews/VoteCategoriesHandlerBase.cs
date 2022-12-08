using iRLeagueApiCore.Common.Models.Reviews;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public class VoteCategoriesHandlerBase<THandler, TRequest> : HandlerBase<THandler, TRequest>
{
    public VoteCategoriesHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    protected virtual async Task<VoteCategoryEntity?> GetVoteCategoryEntityAsync(long leagueId, long catId, CancellationToken cancellationToken)
    {
        return await dbContext.VoteCategories
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.CatId == catId)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual async Task<IEnumerable<VoteCategoryEntity>> GetCategoryEntitiesAsync(long leagueId, CancellationToken cancellationToken)
    {
        return await dbContext.VoteCategories
            .Where(x => x.LeagueId == leagueId)
            .ToListAsync(cancellationToken);
    }

    protected virtual async Task<VoteCategoryModel?> MapToVoteCategoryModels(long leagueId, long catId, CancellationToken cancellationToken)
    {
        return await dbContext.VoteCategories
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.CatId == catId)
            .Select(MapToVoteCategoryModelExpression)
            .FirstOrDefaultAsync(cancellationToken);
    }

    protected virtual Expression<Func<VoteCategoryEntity, VoteCategoryModel>> MapToVoteCategoryModelExpression => cat => new VoteCategoryModel()
    {
        Id = cat.CatId,
        DefaultPenalty = cat.DefaultPenalty,
        Index = cat.Index,
        Text = cat.Text,
    };
}
