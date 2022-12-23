using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.Results;

public record GetResultConfigRequest(long LeagueId, long ResultConfigId) : IRequest<ResultConfigModel>;

public class GetResultConfigHandler : ResultConfigHandlerBase<GetResultConfigHandler, GetResultConfigRequest>,
    IRequestHandler<GetResultConfigRequest, ResultConfigModel>
{
    public GetResultConfigHandler(ILogger<GetResultConfigHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetResultConfigRequest>> validators) :
        base(logger, dbContext, validators)
    {
    }

    public async Task<ResultConfigModel> Handle(GetResultConfigRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getResultConfig = await MapToResultConfigModel(request.LeagueId, request.ResultConfigId, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getResultConfig;
    }
}
