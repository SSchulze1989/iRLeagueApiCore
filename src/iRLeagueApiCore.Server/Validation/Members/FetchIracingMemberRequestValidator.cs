using iRLeagueApiCore.Server.Handlers.Members;

namespace iRLeagueApiCore.Server.Validation.Members;

public class FetchIracingMemberRequestValidator : AbstractValidator<FetchIracingMemberRequest>
{
    private readonly LeagueDbContext dbContext;


    public FetchIracingMemberRequestValidator(LeagueDbContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(x => x.IracingId)
            .NotEmpty()
            .WithMessage("iRacing ID must not be empty");
        RuleFor(x => x.IracingId)
            .Matches(@"^\d+$")
            .WithMessage("iRacing ID must be a valid numeric ID");
    }
}
