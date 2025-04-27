using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record GetRosterListRequest() : IRequest<IEnumerable<RosterInfoModel>>;

public class GetRosterListHandler : RostersHandlerBase<GetRosterListHandler, GetRosterListRequest, IEnumerable<RosterInfoModel>>
{
    public GetRosterListHandler(ILogger<GetRosterListHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetRosterListRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<RosterInfoModel>> Handle(GetRosterListRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getRosters = await GetLeagueRosters(cancellationToken);
        return getRosters;
    }

    private async Task<IEnumerable<RosterInfoModel>> GetLeagueRosters(CancellationToken cancellationToken) {
        return await dbContext.Rosters
            .Select(MapToRosterInfoModelExpression)
            .ToListAsync(cancellationToken);
    }
}
