using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record PutPointSystemRequest(long ResultConfigId, LeagueUser User, PutPointSystemModel Model) : IRequest<PointSystemModel>;

public sealed class PutPointSystemHandler : PointSystemHandlerBase<PutPointSystemHandler, PutPointSystemRequest, PointSystemModel>
{
    public PutPointSystemHandler(ILogger<PutPointSystemHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<PutPointSystemRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<PointSystemModel> Handle(PutPointSystemRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var putResultConfig = await GetResultConfigEntity(request.ResultConfigId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        putResultConfig = await MapToResultConfigEntityAsync(request.User, request.Model, putResultConfig, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
        var getResultConfig = await MapToResultConfigModel(putResultConfig.PointSystemId, cancellationToken)
            ?? throw new InvalidOperationException("Created resource was not found");
        return getResultConfig;
    }
}
