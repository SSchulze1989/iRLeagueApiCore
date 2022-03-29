using iRLeagueApiCore.Communication.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public class PutScoringRequest : PostScoringRequest, IRequest<GetScoringModel>
    {
        public long ScoringId { get; set; }
        public PutScoringRequest(long leagueId, long scoringId) : base(leagueId)
        {
            ScoringId = scoringId;
        }
    }

    public class PutScoringHandler : IRequestHandler<PutScoringRequest, GetScoringModel>
    {
        public Task<GetScoringModel> Handle(PutScoringRequest request, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
