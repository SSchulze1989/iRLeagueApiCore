using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record GetMemberRequest(long MemberId, bool IncludeProfile) : IRequest<MemberModel>;

public class GetMemberHandler : MembersHandlerBase<GetMemberHandler, GetMemberRequest, MemberModel>
{
    public GetMemberHandler(ILogger<GetMemberHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetMemberRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<MemberModel> Handle(GetMemberRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken).ConfigureAwait(false);
        var getMember = await MapToMemberModel(request.MemberId, includeProfile: request.IncludeProfile, cancellationToken: cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getMember;
    }
}
