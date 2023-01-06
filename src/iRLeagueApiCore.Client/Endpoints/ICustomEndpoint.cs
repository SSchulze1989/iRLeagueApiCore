using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client.Endpoints;

public interface ICustomEndpoint : IPostEndpoint<object, object>, IPostEndpoint<object>, IGetEndpoint<object>, IPutEndpoint<object, object>, IDeleteEndpoint
{
    Task<ClientActionResult<IEnumerable<object>>> GetAll(CancellationToken cancellationToken = default);
}
