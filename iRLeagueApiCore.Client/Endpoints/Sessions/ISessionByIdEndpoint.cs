using iRLeagueApiCore.Client.Endpoints.Results;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface ISessionByIdEndpoint : IUpdateEndpoint<SessionModel, PutSessionModel>
    {
        IResultsEndpoint Results();
    }
}