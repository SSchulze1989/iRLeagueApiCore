using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client.Endpoints;

public interface IPutEndpoint<TResult, TModel>
{
    Task<ClientActionResult<TResult>> Put(TModel model, CancellationToken cancellationToken = default);
}
