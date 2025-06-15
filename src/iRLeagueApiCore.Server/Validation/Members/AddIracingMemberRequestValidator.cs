using iRLeagueApiCore.Server.Handlers.Members;

namespace iRLeagueApiCore.Server.Validation.Members;

public class AddIracingMemberRequestValidator : AbstractValidator<AddIracingMemberRequest>
{
    private readonly LeagueDbContext dbContext;


    public AddIracingMemberRequestValidator(LeagueDbContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(x => x.IracingId)
            .NotEmpty()
            .WithMessage("iRacing ID must not be empty");
        RuleFor(x => x.IracingId)
            .Matches(@"^\d+$")
            .WithMessage("iRacing ID must be a valid numeric ID");
        RuleFor(x => x.IracingId)
            .MustAsync(MustNotExist)
            .WithMessage("Member with this iRacing ID already exists");
    }

    private async Task<bool> MustNotExist(string id, CancellationToken cancellationToken)
    {
        // Check if the member already exists in the database  
        return !await dbContext.LeagueMembers.AnyAsync(m => m.Member.IRacingId == id, cancellationToken);
    }
}
