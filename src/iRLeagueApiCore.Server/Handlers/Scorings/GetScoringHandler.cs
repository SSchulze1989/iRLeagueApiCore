using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Scorings;

public record GetScoringRequest(long ScoringId) : IRequest<ScoringModel>;

public sealed class GetScoringHandler : ScoringHandlerBase<GetScoringHandler,  GetScoringRequest, ScoringModel>
{
    public GetScoringHandler(ILogger<GetScoringHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetScoringRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public override async Task<ScoringModel> Handle(GetScoringRequest request, CancellationToken cancellationToken = default)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        return await MapToGetScoringModelAsync(request.ScoringId, cancellationToken) ?? throw new ResourceNotFoundException();
    }
}
