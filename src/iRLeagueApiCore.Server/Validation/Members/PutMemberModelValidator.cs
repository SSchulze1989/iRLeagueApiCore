using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Validation.Members;

public sealed class PutMemberModelValidator : DbContextValidator<PutMemberModel>
{
    public PutMemberModelValidator(ILeagueDbContext dbContext) : base(dbContext)
    {
        RuleFor(x => x.TeamId).MustAsync(TeamMustExist);
    }

    private async Task<bool> TeamMustExist(long? teamId, CancellationToken cancellationToken)
    {
        if (teamId == null || teamId.Value == 0)
        {
            return true;
        }
        return await dbContext.Teams.AnyAsync(x => x.TeamId == teamId.Value, cancellationToken);
    }
}
