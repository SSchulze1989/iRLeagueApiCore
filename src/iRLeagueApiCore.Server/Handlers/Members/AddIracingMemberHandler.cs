using Aydsko.iRacingData;
using Aydsko.iRacingData.Member;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record AddIracingMemberRequest(string IracingId) : IRequest<MemberModel>;

public class AddIracingMemberHandler : MembersHandlerBase<AddIracingMemberHandler, AddIracingMemberRequest, MemberModel>
{
    private readonly IDataClient iRacingDataClient;

    public AddIracingMemberHandler(ILogger<AddIracingMemberHandler> logger, LeagueDbContext dbContext, IDataClient iRacingDataClient, IEnumerable<IValidator<AddIracingMemberRequest>> validators)
        : base(logger, dbContext, validators)
    {
        this.iRacingDataClient = iRacingDataClient;
    }

    public override async Task<MemberModel> Handle(AddIracingMemberRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);

        // fetch member from iRacing
        var memberProfile = await FetchMemberInfo(request.IracingId, iRacingDataClient, cancellationToken)
            ?? throw new ResourceNotFoundException($"Member with iRacing ID {request.IracingId} not found");
        var (firstName, lastName) = ParseFullName(memberProfile.Info.DisplayName);
        // check if member already exists in database
        var member = await dbContext.Members
            .FirstOrDefaultAsync(m => m.IRacingId == request.IracingId, cancellationToken);
        if (member is null)
        {
            member = new MemberEntity()
            {
                IRacingId = request.IracingId,
            };
            dbContext.Members.Add(member);
        };
        member.Firstname = firstName;
        member.Lastname = lastName;
        var newLeagueMember = new LeagueMemberEntity()
        {
            LeagueId = dbContext.LeagueProvider.LeagueId,
            Member = member,
            Number = string.Empty,
            DiscordId = string.Empty,
            CountryFlag = string.Empty,
        };
        dbContext.LeagueMembers.Add(newLeagueMember);
        await dbContext.SaveChangesAsync(cancellationToken);
        var memberModel = await MapToMemberModel(newLeagueMember.MemberId, includeProfile: true, cancellationToken: cancellationToken)
            ?? throw new InvalidOperationException("Newly created member not found in database");
        return memberModel;
    }
}
