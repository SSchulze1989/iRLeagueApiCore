using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;

namespace iRLeagueApiCore.Client.Endpoints.Scorings
{
    public interface IScoringByIdEndpoint : IUpdateEndpoint<ScoringModel, PutScoringModel>
    {
        IPostEndpoint<NoContent, NoContent> AddSession(long sessionId);
        IPostEndpoint<NoContent, NoContent> RemoveSession(long sessionId);
    }
}