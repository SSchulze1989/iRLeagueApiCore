using iRLeagueApiCore.Client.Http;
using iRLeagueApiCore.Client.QueryBuilder;
using iRLeagueApiCore.Client.Results;
using iRLeagueApiCore.Client.ResultsParsing;
using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Client.Endpoints.Results;

internal class ResultsEndpoint : GetAllEndpoint<EventResultModel>,
    IResultsEndpoint, IWithIdEndpoint<IResultByIdEndpoint>, IEventResultsEndpoint, ISeasonResultsEndpoint
{
    public ResultsEndpoint(HttpClientWrapper httpClientWrapper, RouteBuilder routeBuilder) :
        base(httpClientWrapper, routeBuilder)
    {
        RouteBuilder.AddEndpoint("Results");
    }

    public IUpdateEndpoint<RawEventResultModel, RawEventResultModel> Raw()
    {
        return new RawResultsEndpoint(HttpClientWrapper, RouteBuilder);
    }

    public IPutEndpoint<RawResultRowModel, RawResultRowModel> ModifyResultRow(long resultRowId, bool triggerCalculation = false)
    {
        var routeBuilder = RouteBuilder.Copy();
        routeBuilder.AddEndpoint("ModResultRow");
        routeBuilder.AddParameter(resultRowId);
        routeBuilder.WithParameters(x => x.Add("triggerCaculation", triggerCalculation));
        return new PutEndpoint<RawResultRowModel, RawResultRowModel>(HttpClientWrapper, routeBuilder);
    }

    public IPostEndpoint<bool, ParseSimSessionResult> Upload()
    {
        return new UploadResultEndpoint(HttpClientWrapper, RouteBuilder);
    }

    public IResultByIdEndpoint WithId(long id)
    {
        return new ResultByIdEndpoint(HttpClientWrapper, RouteBuilder, id);
    }

    IPostEndpoint<bool> IEventResultsEndpoint.Calculate()
    {
        return new CalculateEndpoint(HttpClientWrapper, RouteBuilder);
    }

    async Task<ClientActionResult<NoContent>> IDeleteEndpoint.Delete(CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.DeleteAsClientActionResult(QueryUrl, cancellationToken);
    }

    IFetchResultsEndpoint IEventResultsEndpoint.Fetch()
    {
        return new FetchEndpoint(HttpClientWrapper, RouteBuilder);
    }

    async Task<ClientActionResult<IEnumerable<SeasonEventResultModel>>> IGetEndpoint<IEnumerable<SeasonEventResultModel>>.Get(CancellationToken cancellationToken)
    {
        return await HttpClientWrapper.GetAsClientActionResult<IEnumerable<SeasonEventResultModel>>(QueryUrl, cancellationToken);
    }

    IGetEndpoint<IEnumerable<EventResultModel>> IResultsEndpoint.Latest()
    {
        return new GetLatestEndpoint<IEnumerable<EventResultModel>>(HttpClientWrapper, RouteBuilder);
    }
}
