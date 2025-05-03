using iRLeagueApiCore.Common.Models.Rosters;
using iRLeagueApiCore.Server.Handlers.Seasons;

namespace iRLeagueApiCore.Server.Validation.Rosters;

public sealed class PostRosterModelValidator : DbContextValidator<PostRosterModel>
{
    public PostRosterModelValidator(ILeagueDbContext dbContext) : base(dbContext)
    {
        RuleForEach(x => x.RosterEntries)
            .MustAsync(MemberExists)
            .WithMessage("MemberId must target a valid Id of an existing league member")
            .MustAsync(TeamExists)
            .WithMessage("TeamId must target a valid TeamId of an existing team or be \"null\" for no team");
    }

    private async Task<bool> MemberExists(RosterEntryModel entry, CancellationToken cancellationToken)
    {
        if (entry.MemberId <= 0)
        {
            return false;
        }
        return await dbContext.LeagueMembers
            .AnyAsync(x => x.MemberId == entry.MemberId);
    }

    private async Task<bool> TeamExists(RosterEntryModel entry, CancellationToken cancellationToken)
    {
        if (entry.TeamId is null)
        {
            return true;
        }
        if (entry.TeamId.Value <= 0)
        {
            return false;
        }
        return await dbContext.Teams
            .AnyAsync(x => x.TeamId == entry.TeamId.Value);
    }
}
