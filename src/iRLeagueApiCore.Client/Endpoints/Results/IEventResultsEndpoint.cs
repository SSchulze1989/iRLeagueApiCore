using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results
{
    public interface IEventResultsEndpoint : IGetAllEndpoint<EventResultModel>
    {
        IPostEndpoint<string> Upload();
    }
}
