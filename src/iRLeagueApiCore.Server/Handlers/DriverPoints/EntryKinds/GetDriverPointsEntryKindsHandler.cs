using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints.EntryKinds;

public record GetDriverPointsEntryKindsRequest() : IRequest<IEnumerable<DriverPointsEntryKindModel>>;

public sealed class GetDriverPointsEntryKindsHandler : HandlerBase<GetDriverPointsEntryKindsHandler, GetDriverPointsEntryKindsRequest, IEnumerable<DriverPointsEntryKindModel>>
{
    private readonly IDriverPointsService service;

    public GetDriverPointsEntryKindsHandler(ILogger<GetDriverPointsEntryKindsHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDriverPointsEntryKindsRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<IEnumerable<DriverPointsEntryKindModel>> Handle(GetDriverPointsEntryKindsRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetCurrentLeagueEntityAsync(cancellationToken) ?? throw new ResourceNotFoundException("League not found");
        var kinds = await service.GetEntryKindsAsync(league.Id);
        return kinds;
    }
}
