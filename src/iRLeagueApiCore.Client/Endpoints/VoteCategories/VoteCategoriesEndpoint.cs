using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Common.Models.Reviews;

namespace iRLeagueApiCore.Client.Endpoints.VoteCategories;

internal sealed class VoteCategoriesEndpoint : GetAllEndpoint<VoteCategoryModel>, IVoteCategoriesEndpoint
{
    public VoteCategoriesEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("VoteCategories");
    }
}
