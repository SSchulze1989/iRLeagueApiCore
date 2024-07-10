using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Standings;

public record GetDropweekOverrideRequest(long StandingConfigId, long ScoredResultRowId) : IRequest<DropweekOverrideModel>;

public class GetDropweekOverrideHandler : StandingsHandlerBase<GetDropweekOverrideHandler, GetDropweekOverrideRequest, DropweekOverrideModel>
{
    public GetDropweekOverrideHandler(ILogger<GetDropweekOverrideHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDropweekOverrideRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<DropweekOverrideModel> Handle(GetDropweekOverrideRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var dropweek = await MapToDropweekOverrideModel(request.StandingConfigId, request.ScoredResultRowId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return dropweek;
    }
}
