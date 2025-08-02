using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetPointSystemsFromLeagueRequest() : IRequest<IEnumerable<PointSystemModel>>;

public sealed class GetPointSystemsFromLeagueHandler : PointSystemHandlerBase<GetPointSystemsFromLeagueHandler, GetPointSystemsFromLeagueRequest, IEnumerable<PointSystemModel>>
{
    public GetPointSystemsFromLeagueHandler(ILogger<GetPointSystemsFromLeagueHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetPointSystemsFromLeagueRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<IEnumerable<PointSystemModel>> Handle(GetPointSystemsFromLeagueRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getResults = await MapToGetResultConfigsFromLeagueAsync(cancellationToken);
        return getResults;
    }

    private async Task<IEnumerable<PointSystemModel>> MapToGetResultConfigsFromLeagueAsync(CancellationToken cancellationToken)
    {
        return await dbContext.ResultConfigurations
            .OrderBy(x => x.PointSystemId)
            .Select(MapToResultConfigModelExpression)
            .ToListAsync(cancellationToken);
    }
}
