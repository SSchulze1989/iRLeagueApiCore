using iRLeagueApiCore.Communication.Models;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record PutScoringRequest(long LeagueId, long ScoringId, PutScoringModel Model) : IRequest<GetScoringModel>;

    public class PutScoringHandler : IRequestHandler<PutScoringRequest, GetScoringModel>
    {
        public Task<GetScoringModel> Handle(PutScoringRequest request, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
