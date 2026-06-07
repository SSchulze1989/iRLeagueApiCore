using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints.EntryKinds;

public record PostDriverPointsEntryKindRequest(CreateDriverPointsEntryKindModel Model) : IRequest<DriverPointsEntryKindModel>;

public sealed class PostDriverPointsEntryKindHandler : HandlerBase<PostDriverPointsEntryKindHandler, PostDriverPointsEntryKindRequest, DriverPointsEntryKindModel>
{
    private readonly IDriverPointsService service;

    public PostDriverPointsEntryKindHandler(ILogger<PostDriverPointsEntryKindHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostDriverPointsEntryKindRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<DriverPointsEntryKindModel> Handle(PostDriverPointsEntryKindRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetCurrentLeagueEntityAsync(cancellationToken) ?? throw new ResourceNotFoundException("League not found");
        var kind = await service.CreateEntryKindAsync(league.Id, request.Model);
        return kind;
    }
}
