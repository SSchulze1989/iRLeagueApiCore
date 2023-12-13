using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.Endpoints.Bonuses;
internal sealed class BonusesEndpoint : PostEndpoint<AddBonusModel, PostAddBonusModel>, IBonusesEndpoint, IPostEndpoint<AddBonusModel, PostAddBonusModel>
{
    public BonusesEndpoint(HttpClientWrapper httpClient, RouteBuilder routeBuilder) 
        : base(httpClient, routeBuilder, "Bonuses")
    {
    }

    public IUpdateEndpoint<AddBonusModel, PutAddBonusModel> WithId(long id)
    {
        return new UpdateEndpoint<AddBonusModel, PutAddBonusModel>(HttpClientWrapper, RouteBuilder);
    }
}
