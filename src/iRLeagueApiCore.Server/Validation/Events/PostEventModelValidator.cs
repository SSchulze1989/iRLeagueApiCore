using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Events;

public sealed class PostEventModelValidator : AbstractValidator<PostEventModel>
{
    private readonly LeagueDbContext dbContext;

    public PostEventModelValidator(LeagueDbContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(x => x.Sessions)
            .NotNull()
            .WithMessage("Sessions required");
        RuleForEach(x => x.Sessions)
            .MustAsync(SessionIdValid)
            .WithMessage("Session id must be either 0 or target a valid SessionEntity.SessionId");
    }

    public async Task<bool> SessionIdValid(SessionModel session, CancellationToken cancellationToken)
    {
        var sessionId = session.SessionId;
        var result = sessionId == 0 ||
            await dbContext.Sessions
            .Where(x => x.SessionId == session.SessionId)
            .AnyAsync();
        return result;
    }
}
