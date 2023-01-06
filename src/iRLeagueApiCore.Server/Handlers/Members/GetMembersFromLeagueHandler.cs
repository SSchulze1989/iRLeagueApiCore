using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record GetMembersFromLeagueRequest(long LeagueId) : IRequest<IEnumerable<MemberModel>>;

public sealed class GetMembersFromLeagueHandler : MembersHandlerBase<GetMembersFromLeagueHandler, GetMembersFromLeagueRequest>,
    IRequestHandler<GetMembersFromLeagueRequest, IEnumerable<MemberModel>>
{
    public GetMembersFromLeagueHandler(ILogger<GetMembersFromLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetMembersFromLeagueRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<MemberModel>> Handle(GetMembersFromLeagueRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getMembers = await MapToMemberModels(request.LeagueId, cancellationToken);
        return getMembers;
    }

    private async Task<IEnumerable<MemberModel>> MapToMemberModels(long leagueId, CancellationToken cancellationToken)
    {
        return await dbContext.LeagueMembers
            .Where(x => x.LeagueId == leagueId)
            .Select(MapToMemberModelExpression)
            .ToListAsync(cancellationToken);
    }
}
