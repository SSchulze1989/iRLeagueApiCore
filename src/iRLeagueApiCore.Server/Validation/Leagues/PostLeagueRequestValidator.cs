using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Validation.Leagues;

public sealed class PostLeagueRequestValidator : AbstractValidator<PostLeagueRequest>
{
    private const int maxLeagues = 3;
    private readonly LeagueDbContext dbContext;

    public PostLeagueRequestValidator(PostLeagueModelValidator modelValidator, LeagueDbContext dbContext)
    {
        this.dbContext = dbContext;
        RuleFor(x => x.Model)
            .SetValidator(modelValidator);
        RuleFor(x => x.User)
            .MustAsync(HaveLessThanMaximumAllowedLeagues)
            .WithMessage("User is already the owner for the maximum allowed number of leagues");
    }

    private async Task<bool> HaveLessThanMaximumAllowedLeagues(LeagueUser user, CancellationToken cancellationToken)
    {
        var userLeagueCount = await dbContext.Leagues
            .Where(x => x.CreatedByUserId == user.Id)
            .CountAsync(cancellationToken);
        return userLeagueCount < maxLeagues;
    }
}
