using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    public interface IResultConfigsEndpoint : IPostGetAllEndpoint<ResultConfigModel, PostResultConfigModel>, IWithIdEndpoint<IResultConfigByIdEndpoint>
    {
    }
}
