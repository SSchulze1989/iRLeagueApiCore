using iRLeagueApiCore.Client.Results;

namespace iRLeagueApiCore.Client.Endpoints
{
    public interface IDeleteEndpoint
    {
        Task<ClientActionResult<NoContent>> Delete(CancellationToken cancellationToken = default);
    }
}