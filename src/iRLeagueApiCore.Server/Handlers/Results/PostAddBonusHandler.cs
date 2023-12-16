using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record PostAddBonusRequest(long ScoredResultRowId, PostAddBonusModel Bonus) : IRequest<AddBonusModel>;

public class PostAddBonusHandler : ResultHandlerBase<PostAddBonusHandler, PostAddBonusRequest>, IRequestHandler<PostAddBonusRequest, AddBonusModel>
{
    public PostAddBonusHandler(ILogger<PostAddBonusHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostAddBonusRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<AddBonusModel> Handle(PostAddBonusRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken).ConfigureAwait(false);

        var postBonus = await CreateAddBonusEntity(request.ScoredResultRowId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        postBonus = MapToAddBonusEntity(postBonus, request.Bonus);
        await dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        var getBonus = await MapToAddBonusModel(postBonus.AddBonusId, cancellationToken).ConfigureAwait(false)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getBonus;
    }

    private async Task<AddBonusEntity?> CreateAddBonusEntity(long scoredResultRowId, CancellationToken cancellationToken)
    {
        var row = await dbContext.ScoredResultRows
            .Where(x => x.ScoredResultRowId == scoredResultRowId)
            .FirstOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return null;
        }

        var bonusEntity = new AddBonusEntity()
        {
            LeagueId = row.LeagueId,
            ScoredResultRow = row,
            ScoredResultRowId = scoredResultRowId,
        };
        dbContext.AddBonuses.Add(bonusEntity);

        return bonusEntity;
    }
}
