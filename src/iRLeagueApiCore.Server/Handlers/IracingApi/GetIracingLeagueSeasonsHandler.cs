using Aydsko.iRacingData;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Server.Handlers.IracingApi;

public record GetIracingLeagueSeasonsRequest(int iracingLeagueId) : IRequest<IEnumerable<IracingLeagueSeasonModel>>;

public class GetIracingLeagueSeasonsHandler : IracingApiHandlerBase<GetIracingLeagueSeasonsHandler, GetIracingLeagueSeasonsRequest, IEnumerable<IracingLeagueSeasonModel>>
{
    public GetIracingLeagueSeasonsHandler(ILogger<GetIracingLeagueSeasonsHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetIracingLeagueSeasonsRequest>> validators, IDataClient dataClient) :
        base(logger, dbContext, validators, dataClient)
    {
    }

    public override Task<IEnumerable<IracingLeagueSeasonModel>> Handle(GetIracingLeagueSeasonsRequest request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
