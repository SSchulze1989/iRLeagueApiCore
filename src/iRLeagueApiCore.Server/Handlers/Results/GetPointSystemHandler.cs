using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetPointSystemRequest(long ResultConfigId) : IRequest<PointSystemModel>;

public sealed class GetPointSystemHandler : PointSystemHandlerBase<GetPointSystemHandler, GetPointSystemRequest, PointSystemModel>
{
    public GetPointSystemHandler(ILogger<GetPointSystemHandler> logger, LeagueDbContext dbContext,
        IEnumerable<IValidator<GetPointSystemRequest>> validators)
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<PointSystemModel> Handle(GetPointSystemRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getResultConfig = await MapToResultConfigModel(request.ResultConfigId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getResultConfig;
    }
}
