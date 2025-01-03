using iRLeagueApiCore.Common.Models;
using System.Linq.Expressions;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetRawResultsFromEventRequest(long EventId) : IRequest<RawEventResultModel>;

public class GetRawResultsFromEventHandler : ResultHandlerBase<GetRawResultsFromEventHandler, GetRawResultsFromEventRequest, RawEventResultModel>
{
    public GetRawResultsFromEventHandler(ILogger<GetRawResultsFromEventHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetRawResultsFromEventRequest>> validators) : base(logger, dbContext, validators)
    {
    }

    public override async Task<RawEventResultModel> Handle(GetRawResultsFromEventRequest request, CancellationToken cancellationToken)
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
