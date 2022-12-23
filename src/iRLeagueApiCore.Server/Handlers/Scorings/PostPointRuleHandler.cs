using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record PostPointRuleRequest(long LeagueId, LeagueUser User, PostPointRuleModel Model) : IRequest<PointRuleModel>;

    public class PostPointRuleHandler : PointRuleHandlerBase<PostPointRuleHandler, PostPointRuleRequest>,
        IRequestHandler<PostPointRuleRequest, PointRuleModel>
    {
        public PostPointRuleHandler(ILogger<PostPointRuleHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostPointRuleRequest>> validators) :
            base(logger, dbContext, validators)
        {
        }

        public async Task<PointRuleModel> Handle(PostPointRuleRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var postPointRule = await CreatePointRuleEntityAsync(request.User, request.LeagueId, cancellationToken);
            postPointRule = await MapToPointRuleEntityAsync(request.User, request.Model, postPointRule, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getPointRule = await MapToPointRuleModel(postPointRule.LeagueId, postPointRule.PointRuleId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getPointRule;
        }

        private async Task<PointRuleEntity> CreatePointRuleEntityAsync(LeagueUser user, long leagueId, CancellationToken cancellationToken)
        {
            var league = await dbContext.Leagues
                .FirstOrDefaultAsync(x => x.Id == leagueId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            var pointRule = CreateVersionEntity(user, new PointRuleEntity());
            league.PointRules.Add(pointRule);
            return pointRule;
        }
    }
}
