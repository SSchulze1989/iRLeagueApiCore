using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints
{
    public interface IPostGetAllEndpoint<TResult, TPost> : IPostEndpoint<TResult, TPost>, IGetAllEndpoint<TResult>
    {
    }
}
