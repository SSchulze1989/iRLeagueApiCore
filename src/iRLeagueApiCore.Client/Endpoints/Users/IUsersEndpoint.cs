using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Users;
using System.Collections.Generic;

namespace iRLeagueApiCore.Client.Endpoints.Users
{
    public interface IUsersEndpoint
    {
        IUserByIdEndpoint WithId(string id);
        IPostEndpoint<IEnumerable<UserModel>, SearchModel> Search();
    }
}
