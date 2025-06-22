using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record GetRosterListRequest(bool IncludeArchived = false) : IRequest<IEnumerable<RosterInfoModel>>;

public class GetRosterListHandler : RostersHandlerBase<GetRosterListHandler, GetRosterListRequest, IEnumerable<RosterInfoModel>>
{
    public GetRosterListHandler(ILogger<GetRosterListHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetRosterListRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<RosterInfoModel>> Handle(GetRosterListRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getRosters = await GetLeagueRosters(request.IncludeArchived, cancellationToken);
        return getRosters;
    }

    private async Task<IEnumerable<RosterInfoModel>> GetLeagueRosters(bool includeArchived, CancellationToken cancellationToken) {
        return await dbContext.Rosters
            .Where(x => !x.IsArchived || includeArchived)
            .Select(MapToRosterInfoModelExpression)
            .ToListAsync(cancellationToken);
    }
}
