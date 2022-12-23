using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client.Endpoints;

public interface IGetEndpoint<T>
{
    Task<ClientActionResult<T>> Get(CancellationToken cancellationToken = default);
}
