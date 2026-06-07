using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Services.DriverPoints;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints.EntryKinds;

public record PutDriverPointsEntryKindRequest(long EntryKindId, UpdateDriverPointsEntryKindModel Model) : IRequest<DriverPointsEntryKindModel>;

public sealed class PutDriverPointsEntryKindHandler : HandlerBase<PutDriverPointsEntryKindHandler, PutDriverPointsEntryKindRequest, DriverPointsEntryKindModel>
{
    private readonly IDriverPointsService service;

    public PutDriverPointsEntryKindHandler(ILogger<PutDriverPointsEntryKindHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutDriverPointsEntryKindRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<DriverPointsEntryKindModel> Handle(PutDriverPointsEntryKindRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        await service.UpdateEntryKindAsync(request.EntryKindId, request.Model);
        var kind = await service.GetEntryKindAsync(request.EntryKindId);
        if (kind == null)
        {
            throw new ResourceNotFoundException($"EntryKind {request.EntryKindId} not found");
        }
        return kind;
    }
}
