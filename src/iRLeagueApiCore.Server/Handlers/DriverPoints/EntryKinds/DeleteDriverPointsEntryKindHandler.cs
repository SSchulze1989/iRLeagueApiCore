using iRLeagueApiCore.Common.Models.DriverPoints;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Services.DriverPoints;
using MediatR;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints.EntryKinds;

public record DeleteDriverPointsEntryKindRequest(long EntryKindId) : IRequest<Unit>;

public sealed class DeleteDriverPointsEntryKindHandler : HandlerBase<DeleteDriverPointsEntryKindHandler, DeleteDriverPointsEntryKindRequest, Unit>
{
    private readonly IDriverPointsService service;

    public DeleteDriverPointsEntryKindHandler(ILogger<DeleteDriverPointsEntryKindHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteDriverPointsEntryKindRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<Unit> Handle(DeleteDriverPointsEntryKindRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        await service.DeleteEntryKindAsync(request.EntryKindId);
        return Unit.Value;
    }
}
