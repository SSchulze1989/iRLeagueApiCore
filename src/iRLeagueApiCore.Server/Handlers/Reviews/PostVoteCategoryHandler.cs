﻿using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Models;
using Microsoft.OpenApi.Any;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record PostVoteCategoryRequest(long LeagueId, LeagueUser User, PostVoteCategoryModel Model) : IRequest<VoteCategoryModel>;

public sealed class PostVoteCategoryHandler : VoteCategoriesHandlerBase<PostVoteCategoryHandler, PostVoteCategoryRequest>, 
    IRequestHandler<PostVoteCategoryRequest, VoteCategoryModel>
{
    public PostVoteCategoryHandler(ILogger<PostVoteCategoryHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostVoteCategoryRequest>> validators) : 
        base(logger, dbContext, validators)
    {
    }

    public async Task<VoteCategoryModel> Handle(PostVoteCategoryRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postVoteCategory = await CreateVoteCategoryEntityAsync(request.LeagueId, cancellationToken);
        postVoteCategory = await MapToVoteCategoryEntityAsync(request.Model, postVoteCategory, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getVoteCategory = await MapToVoteCategoryModel(request.LeagueId, postVoteCategory.CatId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource not found");
        return getVoteCategory;
    }

    private async Task<VoteCategoryEntity> CreateVoteCategoryEntityAsync(long leagueId, CancellationToken cancellationToken)
    {
        var league = await dbContext.Leagues
            .Where(x => x.Id == leagueId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var voteCategory = new VoteCategoryEntity();
        league.VoteCategories.Add(voteCategory);
        dbContext.VoteCategories.Add(voteCategory);
        return voteCategory;
    }
}
