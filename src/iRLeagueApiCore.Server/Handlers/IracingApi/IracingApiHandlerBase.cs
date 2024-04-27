

using Aydsko.iRacingData;

namespace iRLeagueApiCore.Server.Handlers.IracingApi;

public abstract class IracingApiHandlerBase<THandler, TRequest, TResponse> : HandlerBase<THandler, TRequest, TResponse> where TRequest : IRequest<TResponse>
{
    protected readonly IDataClient dataClient;

    public IracingApiHandlerBase(ILogger<THandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<TRequest>> validators, IDataClient dataClient) :
        base(logger, dbContext, validators)
    {
        this.dataClient = dataClient;
    }
}
