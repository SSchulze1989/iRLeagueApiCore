using iRLeagueApiCore.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Sessions
{
    public interface ISessionsEndpoint : IPostEndpoint<GetSessionModel, PostSessionModel>, IGetAllEndpoint<GetSessionModel>, IWithIdEndpoint<ISessionByIdEndpoint>
    {

    }
}
