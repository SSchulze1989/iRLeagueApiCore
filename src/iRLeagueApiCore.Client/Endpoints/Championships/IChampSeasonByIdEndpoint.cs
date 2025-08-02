using iRLeagueApiCore.Client.Endpoints;
using iRLeagueApiCore.Common.Models;

public interface IChampSeasonByIdEndpoint : IUpdateEndpoint<ChampSeasonModel, PutChampSeasonModel>
{
    public IPostEndpoint<PointSystemModel, PostPointSystemModel> ResultConfigs();
}