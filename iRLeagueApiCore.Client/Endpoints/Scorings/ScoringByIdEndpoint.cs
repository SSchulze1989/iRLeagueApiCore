using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System.Net.Http;

namespace iRLeagueApiCore.Client.Endpoints.Scorings
{
    internal class ScoringByIdEndpoint : UpdateEndpoint<GetScoringModel, PutScoringModel>, IScoringByIdEndpoint
    {
        public ScoringByIdEndpoint(HttpClient httpClient, RouteBuilder routeBuilder, long scoringId) : 
            base(httpClient, routeBuilder)
        {
            RouteBuilder.AddParameter(scoringId);
        }

        IPostEndpoint<NoContent, NoContent> IScoringByIdEndpoint.AddSession(long sessionId)
        {
            var addSessionBuilder = RouteBuilder.Copy();
            addSessionBuilder.AddEndpoint("AddSession");
            addSessionBuilder.AddParameter(sessionId);
            return new PostEndpoint<NoContent, NoContent>(HttpClient, addSessionBuilder);
        }

        IPostEndpoint<NoContent, NoContent> IScoringByIdEndpoint.RemoveSession(long sessionId)
        {
            var removeSessionBuilder = RouteBuilder.Copy();
            removeSessionBuilder.AddEndpoint("RemoveSession");
            removeSessionBuilder.AddParameter(sessionId);
            return new PostEndpoint<NoContent, NoContent>(HttpClient, removeSessionBuilder);
        }
    }
}