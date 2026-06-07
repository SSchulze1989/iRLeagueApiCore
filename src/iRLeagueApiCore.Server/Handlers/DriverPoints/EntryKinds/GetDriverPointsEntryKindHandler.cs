using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints.EntryKinds;

public record GetDriverPointsEntryKindRequest(long EntryKindId) : IRequest<DriverPointsEntryKindModel>;

public sealed class GetDriverPointsEntryKindHandler : HandlerBase<GetDriverPointsEntryKindHandler, GetDriverPointsEntryKindRequest, DriverPointsEntryKindModel>
{
    private readonly IDriverPointsService service;

    public GetDriverPointsEntryKindHandler(ILogger<GetDriverPointsEntryKindHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetDriverPointsEntryKindRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<DriverPointsEntryKindModel> Handle(GetDriverPointsEntryKindRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var kind = await service.GetEntryKindAsync(request.EntryKindId);
        if (kind == null)
        {
            throw new ResourceNotFoundException($"EntryKind {request.EntryKindId} not found");
        }
        return kind;
    }
}
