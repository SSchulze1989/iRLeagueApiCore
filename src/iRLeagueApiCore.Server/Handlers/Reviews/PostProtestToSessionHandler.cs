using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record PostProtestToSessionRequest(long LeagueId, long SessionId, LeagueUser User, PostProtestModel Model) : IRequest<ProtestModel>;

public class PostProtestToSessionHandler : ProtestsHandlerBase<PostProtestToSessionHandler, PostProtestToSessionRequest>, 
    IRequestHandler<PostProtestToSessionRequest, ProtestModel>
{
    public PostProtestToSessionHandler(ILogger<PostProtestToSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostProtestToSessionRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<ProtestModel> Handle(PostProtestToSessionRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postProtest = await CreateProtestEntity(request.LeagueId, request.User, request.SessionId, cancellationToken);
        postProtest = await MapToProtestEntity(request.LeagueId, request.User, request.Model, postProtest, cancellationToken);
        dbContext.Protests.Add(postProtest);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getProtest = await MapToProtestModel(request.LeagueId, postProtest.ProtestId, cancellationToken)
            ?? throw new InvalidOperationException("Could not find created resource");
        return getProtest;
    }

    private async Task<ProtestEntity> CreateProtestEntity(long leagueId, LeagueUser user, long sessionId, CancellationToken cancellationToken)
    {
        var session = await dbContext.Sessions
            .Where(x => x.LeagueId == leagueId)
            .Where(x => x.SessionId == sessionId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var protest = new ProtestEntity()
        {
            LeagueId = session.LeagueId,
            Session = session,
        };
        return protest;
    }
}
