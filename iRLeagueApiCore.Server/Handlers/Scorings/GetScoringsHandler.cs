using iRLeagueApiCore.Communication.Models;
using MediatR;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Scorings
{
    public record GetScoringsRequest(long LeagueId, IEnumerable<long> Ids) : IRequest<IEnumerable<GetScoringModel>>;

    public class GetScoringsHandler : IRequestHandler<GetScoringsRequest, IEnumerable<GetScoringModel>>
    {
        public async Task<IEnumerable<GetScoringModel>> Handle(GetScoringsRequest request, CancellationToken cancellationToken = default)
        {
            throw new System.NotImplementedException();
        }
    }
}
