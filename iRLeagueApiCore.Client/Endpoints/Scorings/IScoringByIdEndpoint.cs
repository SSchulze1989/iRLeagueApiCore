using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;

namespace iRLeagueApiCore.Client.Endpoints.Scorings
{
    public interface IScoringByIdEndpoint : IUpdateEndpoint<GetScoringModel, PutScoringModel>
    {
        public IPostEndpoint<NoContent, NoContent> AddSession(long sessionId);
        public IPostEndpoint<NoContent, NoContent> RemoveSession(long sessionId);
    }
}