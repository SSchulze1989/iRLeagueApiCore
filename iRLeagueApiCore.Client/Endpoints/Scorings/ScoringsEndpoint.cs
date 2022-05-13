using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Communication.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Scorings
{
    internal class ScoringsEndpoint : PostGetAllEndpoint<GetScoringModel, PostScoringModel>, IScoringsEndpoint
    {
        public ScoringsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) : 
            base(httpClientWrapper, routeBuilder)
        {
            RouteBuilder.AddEndpoint("Scorings");
        }

        IScoringByIdEndpoint IWithIdEndpoint<IScoringByIdEndpoint>.WithId(long id)
        {
            return new ScoringByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
        }
    }
}
