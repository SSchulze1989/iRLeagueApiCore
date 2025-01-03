using iRLeagueApiCore.Common.Models.Standings;

namespace iRLeagueApiCore.Client.Endpoints.Standings;
public interface IEventStandingsEndpoint : IGetAllEndpoint<StandingsModel>
{
    IPostEndpoint<bool> Calculate();
}
