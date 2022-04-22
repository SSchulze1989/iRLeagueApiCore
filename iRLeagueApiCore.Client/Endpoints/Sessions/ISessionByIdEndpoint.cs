using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Communication.Models;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface ISessionByIdEndpoint : IUpdateEndpoint<GetSessionModel, PutSessionModel>
    {
        public IResultsEndpoint Results();
    }
}