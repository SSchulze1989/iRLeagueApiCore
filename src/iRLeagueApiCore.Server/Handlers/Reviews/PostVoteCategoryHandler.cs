using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record PostVoteCategoryRequest(PostVoteCategoryModel Model) : IRequest<VoteCategoryModel>;

public sealed class PostVoteCategoryHandler : VoteCategoriesHandlerBase<PostVoteCategoryHandler,  PostVoteCategoryRequest, VoteCategoryModel>
{
    public PostVoteCategoryHandler(ILogger<PostVoteCategoryHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostVoteCategoryRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public override async Task<VoteCategoryModel> Handle(PostVoteCategoryRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postVoteCategory = await CreateVoteCategoryEntityAsync(cancellationToken);
        postVoteCategory = await MapToVoteCategoryEntityAsync(request.Model, postVoteCategory, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getVoteCategory = await MapToVoteCategoryModel(postVoteCategory.CatId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource not found");
        return getVoteCategory;
    }

    private async Task<VoteCategoryEntity> CreateVoteCategoryEntityAsync(CancellationToken cancellationToken)
    {
        var league = await GetCurrentLeagueEntityAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var voteCategory = new VoteCategoryEntity();
        league.VoteCategories.Add(voteCategory);
        dbContext.VoteCategories.Add(voteCategory);
        return voteCategory;
    }
}
