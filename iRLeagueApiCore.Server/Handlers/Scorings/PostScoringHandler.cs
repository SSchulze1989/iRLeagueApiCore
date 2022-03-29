using iRLeagueApiCore.Communication.Models;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public class PostScoringRequest : PostScoringModel, IRequest<GetScoringModel>
    {
        public PostScoringRequest(long leagueId)
        {
            LeagueId = leagueId;
        }

        public long LeagueId { get; set; }
    }


    public class PostScoringHandler : IRequestHandler<PostScoringRequest, GetScoringModel>
    {
        public async Task<GetScoringModel> Handle(PostScoringRequest request, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
