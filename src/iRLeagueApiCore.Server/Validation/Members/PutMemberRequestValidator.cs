using iRLeagueApiCore.Server.Handlers.Members;

namespace iRLeagueApiCore.Server.Validation.Members;

public class PutMemberRequestValidator : AbstractValidator<PutMemberRequest>
{
    public PutMemberRequestValidator(PutMemberModelValidator modelValidator)
    {
        RuleFor(x => x.Model).SetValidator(modelValidator);
    }
}
