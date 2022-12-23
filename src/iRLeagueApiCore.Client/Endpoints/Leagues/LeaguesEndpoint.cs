using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Leagues
{
    public class LeaguesEndpoint : ILeaguesEndpoint
    {
        private readonly HttpClientWrapper httpClientWrapper;
        private readonly RouteBuilder routeBuilder;

        private string QueryUrl => routeBuilder.Build();

        public LeaguesEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder)
        {
            this.httpClientWrapper = httpClientWrapper;
            this.routeBuilder = routeBuilder;
            routeBuilder.AddEndpoint("Leagues");
        }

        async Task<ClientActionResult<IEnumerable<LeagueModel>>> IGetEndpoint<IEnumerable<LeagueModel>>.Get(CancellationToken cancellationToken)
        {
            return await httpClientWrapper.GetAsClientActionResult<IEnumerable<LeagueModel>>(QueryUrl, cancellationToken);
        }

        async Task<ClientActionResult<LeagueModel>> ILeaguesEndpoint.Post(PostLeagueModel model, CancellationToken cancellationToken)
        {
            return await httpClientWrapper.PostAsClientActionResult<LeagueModel>(QueryUrl, model, cancellationToken);
        }

        ILeagueByIdEndpoint ILeaguesEndpoint.WithId(long leagueId)
        {
            return new LeagueByIdEndpoint(httpClientWrapper, routeBuilder, leagueId);
        }

        ILeagueByNameEndpoint ILeaguesEndpoint.WithName(string leagueName)
        {
            var withNameBuilder = routeBuilder.Copy();
            withNameBuilder.RemoveLast();
            return new LeagueByNameEndpoint(httpClientWrapper, withNameBuilder, leagueName);
        }
    }
}
