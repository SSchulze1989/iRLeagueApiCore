using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Leagues
{
    public record PostLeagueRequest(LeagueUser User, PostLeagueModel Model) : IRequest<LeagueModel>;

    public class PostLeagueHandler : LeagueHandlerBase<PostLeagueHandler, PostLeagueRequest>, IRequestHandler<PostLeagueRequest, LeagueModel>
    {
        public PostLeagueHandler(ILogger<PostLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostLeagueRequest>> validators)
            : base(logger, dbContext, validators)
        {
        }

        public async Task<LeagueModel> Handle(PostLeagueRequest request, CancellationToken cancellationToken = default)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            _logger.LogInformation("Create league {LeagueName}", request.Model.Name);
            var leagueEntity = MapToLeagueEntity(request.User, request.Model, new LeagueEntity());
            dbContext.Leagues.Add(leagueEntity);
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("League {LeagueName} successfully created", request.Model.Name);
            var getLeague = await MapToGetLeagueModelAsync(leagueEntity.Id, cancellationToken) ?? throw new ResourceNotFoundException("Created resource not found!");
            return getLeague;
        }
    }
}
