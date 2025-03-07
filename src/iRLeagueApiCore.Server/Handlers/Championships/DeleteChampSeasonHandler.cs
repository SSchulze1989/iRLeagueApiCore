﻿namespace iRLeagueApiCore.Server.Handlers.Championships;

public record DeleteChampSeasonRequest(long ChampSeasonId) : IRequest<Unit>;

public sealed class DeleteChampSeasonHandler : ChampSeasonHandlerBase<DeleteChampSeasonHandler, DeleteChampSeasonRequest, Unit>
{
    public DeleteChampSeasonHandler(ILogger<DeleteChampSeasonHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteChampSeasonRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<Unit> Handle(DeleteChampSeasonRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var deleteChampSeason = await GetChampSeasonEntityAsync(request.ChampSeasonId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        deleteChampSeason.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}