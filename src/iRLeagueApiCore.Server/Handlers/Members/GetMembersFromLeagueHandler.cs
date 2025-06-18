using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record GetMembersFromLeagueRequest(bool IncludeProfile = false) : IRequest<IEnumerable<MemberModel>>;

public sealed class GetMembersFromLeagueHandler : MembersHandlerBase<GetMembersFromLeagueHandler, GetMembersFromLeagueRequest, IEnumerable<MemberModel>>
{
    public GetMembersFromLeagueHandler(ILogger<GetMembersFromLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetMembersFromLeagueRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<MemberModel>> Handle(GetMembersFromLeagueRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getMembers = await MapToMemberModels(request.IncludeProfile, cancellationToken);
        return getMembers;
    }

    private async Task<IEnumerable<MemberModel>> MapToMemberModels(bool includeProfile, CancellationToken cancellationToken)
    {
        return await dbContext.LeagueMembers
            .Select(MapToMemberModelExpression(includeProfile: includeProfile))
            .ToListAsync(cancellationToken);
    }
}
