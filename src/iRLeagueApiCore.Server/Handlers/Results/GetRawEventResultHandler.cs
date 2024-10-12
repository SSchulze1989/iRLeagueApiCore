using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetRawEventResultRequest(long EventId) : IRequest<RawEventResultModel>;

public class GetRawEventResultHandler : ResultHandlerBase<GetRawEventResultHandler, GetRawEventResultRequest, RawEventResultModel>
{
    public GetRawEventResultHandler(ILogger<GetRawEventResultHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetRawEventResultRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<RawEventResultModel> Handle(GetRawEventResultRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var result = await dbContext.EventResults
            .Where(x => x.EventId == request.EventId)
            .Select(MapToRawEventResultModelExpression)
            .FirstOrDefaultAsync(cancellationToken)
            ?? throw new ResourceNotFoundException();
        return result;
    }
}
