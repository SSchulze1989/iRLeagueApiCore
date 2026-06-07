using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Services.DriverPoints;
using MediatR;

namespace iRLeagueApiCore.Server.Handlers.DriverPoints;

public record ArchiveDriverPointsEntryRequest(long EntryId) : IRequest<Unit>;

public sealed class ArchiveDriverPointsEntryHandler : HandlerBase<ArchiveDriverPointsEntryHandler, ArchiveDriverPointsEntryRequest, Unit>
{
    private readonly IDriverPointsService service;

    public ArchiveDriverPointsEntryHandler(ILogger<ArchiveDriverPointsEntryHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<ArchiveDriverPointsEntryRequest>> validators, IDriverPointsService service)
        : base(logger, dbContext, validators)
    {
        this.service = service;
    }

    public override async Task<Unit> Handle(ArchiveDriverPointsEntryRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var league = await GetCurrentLeagueEntityAsync(cancellationToken) ?? throw new ResourceNotFoundException("League not found");
        await service.ArchiveEntryAsync(league.Id, request.EntryId, 0);
        return Unit.Value;
    }
}
