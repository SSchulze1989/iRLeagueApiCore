using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record GetLeagueVoteCategoriesRequest(long LeagueId) : IRequest<IEnumerable<VoteCategoryModel>>;

    public sealed class GetLeagueVoteCategoriesHandler : VoteCategoriesHandlerBase<GetLeagueVoteCategoriesHandler, GetLeagueVoteCategoriesRequest>,
        IRequestHandler<GetLeagueVoteCategoriesRequest, IEnumerable<VoteCategoryModel>>
    {
        public GetLeagueVoteCategoriesHandler(ILogger<GetLeagueVoteCategoriesHandler> logger, LeagueDbContext dbContext, 
            IEnumerable<IValidator<GetLeagueVoteCategoriesRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<VoteCategoryModel>> Handle(GetLeagueVoteCategoriesRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);

            var getVoteCategories = await MapToLeagueVoteCategoryModels(request.LeagueId, cancellationToken);
            return getVoteCategories;
        }

        private async Task<IEnumerable<VoteCategoryModel>> MapToLeagueVoteCategoryModels(long leagueId, CancellationToken cancellationToken)
        {
            return await dbContext.VoteCategories
                .Where(x => x.LeagueId == leagueId)
                .Select(MapToVoteCategoryModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
