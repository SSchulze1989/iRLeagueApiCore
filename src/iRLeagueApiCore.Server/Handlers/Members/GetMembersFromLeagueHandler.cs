using iRLeagueApiCore.Common.Models.Members;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record GetMembersFromLeagueRequest(long LeagueId) : IRequest<IEnumerable<MemberInfoModel>>;

public sealed class GetMembersFromLeagueHandler : MembersHandlerBase<GetMembersFromLeagueHandler, GetMembersFromLeagueRequest>,
    IRequestHandler<GetMembersFromLeagueRequest, IEnumerable<MemberInfoModel>>
{
    public GetMembersFromLeagueHandler(ILogger<GetMembersFromLeagueHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetMembersFromLeagueRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public async Task<IEnumerable<MemberInfoModel>> Handle(GetMembersFromLeagueRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getMembers = await MapToMemberModels(request.LeagueId, cancellationToken);
        return getMembers;
    }

    private async Task<IEnumerable<MemberInfoModel>> MapToMemberModels(long leagueId, CancellationToken cancellationToken)
    {
        return await dbContext.LeagueMembers
            .Where(x => x.LeagueId == leagueId)
            .Select(x => x.Member)
            .Select(MapToMemberInfoExpression)
            .ToListAsync(cancellationToken);
    }
}
