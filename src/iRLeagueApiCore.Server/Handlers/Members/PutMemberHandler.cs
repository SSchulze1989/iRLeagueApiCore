using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record PutMemberRequest(long MemberId, PutMemberModel Model) : IRequest<MemberModel>;

public class PutMemberHandler : MembersHandlerBase<PutMemberHandler, PutMemberRequest, MemberModel>
{
    public PutMemberHandler(ILogger<PutMemberHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutMemberRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<MemberModel> Handle(PutMemberRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken).ConfigureAwait(false);
        var putMember = await GetLeagueMemberEntity(request.MemberId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        putMember = await MapToLeagueMemberEntity(putMember, request.Model, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getMember = await MapToMemberModel(putMember.MemberId, includeProfile: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Updated resource not found");
        return getMember;
    }
}
