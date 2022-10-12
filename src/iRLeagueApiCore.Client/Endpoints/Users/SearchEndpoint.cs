using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Users
{
    public class SearchEndpoint : PostEndpoint<IEnumerable<UserModel>, SearchModel>, IPostEndpoint<IEnumerable<UserModel>, SearchModel>
    {
        public SearchEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Search");
        }
    }
}
