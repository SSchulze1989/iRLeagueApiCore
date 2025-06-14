using Aydsko.iRacingData;
using Aydsko.iRacingData.Member;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record  FetchIracingMemberRequest(string IracingId) : IRequest<MemberModel>;

public class FetchIracingMemberHandler : MembersHandlerBase<FetchIracingMemberHandler, FetchIracingMemberRequest, MemberModel>
{
    private readonly DataClient iRacingDataClient;

    public FetchIracingMemberHandler(ILogger<FetchIracingMemberHandler> logger, LeagueDbContext dbContext, DataClient iRacingDataClient, IEnumerable<IValidator<FetchIracingMemberRequest>> validators) 
        : base(logger, dbContext, validators)
    {
        this.iRacingDataClient = iRacingDataClient;
    }

    public override async Task<MemberModel> Handle(FetchIracingMemberRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);

        // fetch member from iRacing
        var memberProfile = await FetchMemberInfo(request.IracingId, iRacingDataClient, cancellationToken)
            ?? throw new ResourceNotFoundException($"Member with iRacing ID {request.IracingId} not found");
        // check if member already exists in league
        var existingMember = await dbContext.LeagueMembers
            .Include(x => x.Member)
            .FirstOrDefaultAsync(x => x.Member.IRacingId == request.IracingId, cancellationToken);
        if (existingMember != null)
        {
            throw new InvalidOperationException("Member already exists in league with ID {existingMember.MemberId} and iRacing ID {existingMember.Member.IRacingId}");
        }

        var (firstName, lastName) = ParseFullName(memberProfile.Info.DisplayName);
        return new MemberModel()
        {
            IRacingId = string.Empty,
            Firstname = firstName,
            Lastname = lastName,
        };
    }
}
