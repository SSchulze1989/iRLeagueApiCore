using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results;

public interface IPointSystemsEndpoint : IGetAllEndpoint<PointSystemModel>, IWithIdEndpoint<IPointSystemByIdEndpoint>
{
}
