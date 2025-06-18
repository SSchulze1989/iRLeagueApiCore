using Aydsko.iRacingData;
using Aydsko.iRacingData.Member;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Members;

public record  FetchIracingMemberRequest(string IracingId) : IRequest<MemberModel>;

public class FetchIracingMemberHandler : MembersHandlerBase<FetchIracingMemberHandler, FetchIracingMemberRequest, MemberModel>
{
    private readonly IDataClient iRacingDataClient;

    public FetchIracingMemberHandler(ILogger<FetchIracingMemberHandler> logger, LeagueDbContext dbContext, IDataClient iRacingDataClient, IEnumerable<IValidator<FetchIracingMemberRequest>> validators) 
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

        var (firstName, lastName) = ParseFullName(memberProfile.Info.DisplayName);
        return new MemberModel()
        {
            IRacingId = memberProfile.CustomerId.ToString(),
            Firstname = firstName,
            Lastname = lastName,
        };
    }
}
