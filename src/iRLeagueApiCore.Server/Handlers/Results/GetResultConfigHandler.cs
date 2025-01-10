using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetResultConfigRequest(long ResultConfigId) : IRequest<ResultConfigModel>;

public sealed class GetResultConfigHandler : ResultConfigHandlerBase<GetResultConfigHandler,  GetResultConfigRequest, ResultConfigModel>
{
    public GetResultConfigHandler(ILogger<GetResultConfigHandler> logger, LeagueDbContext dbContext, 
        IEnumerable<IValidator<GetResultConfigRequest>> validators) 
        : base(logger, dbContext, validators)
    {
    }

    public override async Task<ResultConfigModel> Handle(GetResultConfigRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getResultConfig = await MapToResultConfigModel(request.ResultConfigId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getResultConfig;
    }
}
