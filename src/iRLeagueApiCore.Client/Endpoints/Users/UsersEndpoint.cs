using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Users;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Users
{
    public class UsersEndpoint : GetAllEndpoint<LeagueUserModel>, IUsersEndpoint, ILeagueUsersEndpoint
    {
        public UsersEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) : base(httpClient, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Users");
        }

        public IUserByIdEndpoint WithId(string id)
        {
            return new UserByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }

        IPostEndpoint<IEnumerable<UserModel>, SearchModel> IUsersEndpoint.Search()
        {
            return new SearchEndpoint(HttpClientWrapper, RouteBuilder);
        }

        ILeagueUserByIdEndpoint ILeagueUsersEndpoint.WithId(string id)
        {
            return new UserByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
