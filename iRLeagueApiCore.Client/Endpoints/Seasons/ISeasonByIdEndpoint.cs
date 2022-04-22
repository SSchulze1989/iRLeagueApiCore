using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Communication.Models;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Seasons
{
    public interface ISeasonByIdEndpoint : IUpdateEndpoint<GetSeasonModel, PutSeasonModel>
    {
    }
}