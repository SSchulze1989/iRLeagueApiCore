using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Server.Validation.Reviews;

public class PostReviewModelValidator : AbstractValidator<PostReviewModel>
{
    private readonly LeagueDbContext dbContext;

    public PostReviewModelValidator(LeagueDbContext dbContext)
    {
        this.dbContext = dbContext;

        RuleFor(x => x.InvolvedMembers)
            .Cascade(CascadeMode.Stop)
            .NotEmpty()
            .WithMessage("At least one driver required")
            .MustAsync(EachMemberIsValid)
            .WithMessage("Member does not exist");
        RuleFor(x => x.IncidentKind)
            .NotEmpty()
            .WithMessage("Incident Kind is required");
    }

    private async Task<bool> EachMemberIsValid(IEnumerable<MemberInfoModel> members, CancellationToken cancellationToken)
    {
        var memberIds = members.Select(x => x.MemberId).ToList();
        return await dbContext.Members
            .AnyAsync(x => memberIds.Contains(x.Id), cancellationToken);
    }
}
