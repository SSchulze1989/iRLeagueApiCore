using iRLeagueApiCore.Common.Models.Rosters;

namespace iRLeagueApiCore.Server.Handlers.Rosters;

public record GetRosterRequest(long RosterId) : IRequest<RosterModel>;

public class GetRosterHandler : RostersHandlerBase<GetRosterHandler, GetRosterRequest, RosterModel>
{
    public GetRosterHandler(ILogger<GetRosterHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetRosterRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<RosterModel> Handle(GetRosterRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var roster = await GetRosterModel(request.RosterId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return roster;
    }
}
