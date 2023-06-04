using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record PostPenaltyToResultRequest(long ResultId, long MemberId, PenaltyModel Model) : IRequest<PenaltyModel>;
public class PostPenaltyToResultHandler : ReviewsHandlerBase<PostPenaltyToResultHandler, PostPenaltyToResultRequest>, 
    IRequestHandler<PostPenaltyToResultRequest, PenaltyModel>
{
    public PostPenaltyToResultHandler(ILogger<PostPenaltyToResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostPenaltyToResultRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public async Task<PenaltyModel> Handle(PostPenaltyToResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var postPenalty = await CreatePenalty(request.ResultId, request.MemberId, cancellationToken);
        postPenalty = await MapToAddPenaltyEntity(request.Model, postPenalty, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getPenaltyEntity = await dbContext.AddPenaltys
            .Where(x => x.AddPenaltyId == postPenalty.AddPenaltyId)
            .FirstAsync();
        var getPenalty = await MapToAddPenaltyModel(postPenalty.AddPenaltyId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getPenalty;
    }

    private async Task<AddPenaltyEntity> CreatePenalty(long resultId, long memberId, CancellationToken cancellationToken)
    {
        var result = await dbContext.ScoredSessionResults
            .Include(x => x.ScoredResultRows)
                .ThenInclude(x => x.Member)
            .Where(x => x.SessionResultId == resultId)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        var resultRow = result.ScoredResultRows
            .Where(x => x.Member.Id == memberId)
            .FirstOrDefault()
            ?? throw new ResourceNotFoundException();

        var penalty = new AddPenaltyEntity()
        {
            LeagueId = result.LeagueId,
            ScoredResultRow = resultRow,
        };
        dbContext.AddPenaltys.Add(penalty);

        return penalty;
    }

    private async Task<PenaltyModel?> MapToAddPenaltyModel(long addPenaltyId, CancellationToken cancellationToken)
    {
        // It is required to fetch the whole entity here first because if used witht he expression directly the 
        // value of "Value.Time" is not converted to a TimeSpan and will always have the default value
        // It is not ideal to fetch the whole entities - including ScoredResultRow - but I could not find another workaround
        // --> this might be solved by updating to EF Core 7 with Pomelo
        return (await dbContext.AddPenaltys
            .Include(x => x.ScoredResultRow.Member)
            .Where(x => x.AddPenaltyId == addPenaltyId)
            .ToListAsync(cancellationToken))
            .Select(MapToAddPenaltyModelExpression.Compile())
            .FirstOrDefault();
    }
}
