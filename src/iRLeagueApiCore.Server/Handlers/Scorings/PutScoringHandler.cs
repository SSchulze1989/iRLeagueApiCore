using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Scorings;

public record PutScoringRequest(long LeagueId, long ScoringId, LeagueUser User, PutScoringModel Model) : IRequest<ScoringModel>;

public sealed class PutScoringHandler : ScoringHandlerBase<PutScoringHandler, PutScoringRequest>, IRequestHandler<PutScoringRequest, ScoringModel>
{
    public PutScoringHandler(ILogger<PutScoringHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutScoringRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<ScoringModel> Handle(PutScoringRequest request, CancellationToken cancellationToken = default)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var putScoring = await GetScoringEntityAsync(request.LeagueId, request.ScoringId) ?? throw new ResourceNotFoundException();
        await MapToScoringEntityAsync(request.User, request.LeagueId, request.Model, putScoring);
        await dbContext.SaveChangesAsync();
        var getScoring = await MapToGetScoringModelAsync(request.LeagueId, putScoring.ScoringId)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getScoring;
    }
}
