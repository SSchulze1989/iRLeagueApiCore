using Aydsko.iRacingData;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Leagues;
using iRLeagueApiCore.Server.Models;

namespace iRLeagueApiCore.Server.Handlers.Leagues;

public record SearchIracingLeaguesRequest(string Search, int Page, int PageSize) : IRequest<PaginatedResultModel<IEnumerable<SearchIracingLeagueResultModel>>>;

public class SearchIracingLeaguesHandler : HandlerBase<SearchIracingLeaguesHandler, SearchIracingLeaguesRequest, 
    PaginatedResultModel<IEnumerable<SearchIracingLeagueResultModel>>>, IRequestHandler<SearchIracingLeaguesRequest, PaginatedResultModel<IEnumerable<SearchIracingLeagueResultModel>>>
{
    private readonly IDataClient dataClient;

    public SearchIracingLeaguesHandler(ILogger<SearchIracingLeaguesHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<SearchIracingLeaguesRequest>> validators, IDataClient dataClient)
        : base(logger, dbContext, validators)
    {
        this.dataClient = dataClient;
    }

    public override async Task<PaginatedResultModel<IEnumerable<SearchIracingLeagueResultModel>>> Handle(SearchIracingLeaguesRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var lowerbound = (request.Page - 1) * request.PageSize + 1;
        var upperbound = request.Page * request.PageSize;
        var searchResult = (await dataClient.SearchLeagueDirectoryAsync(new()
        {
            Search = request.Search,
            Lowerbound = lowerbound,
            Upperbound = upperbound,
        }, cancellationToken)).Data;
        var leagues = searchResult.Items.Select(x => new SearchIracingLeagueResultModel()
        {
            IracingLeagueId = x.LeagueId,
            Name = x.LeagueName,
            OwnerDisplayName = x.Owner.DisplayName,
            OwnerId = x.OwnerId,
        }).ToArray();
        var resultPageSize = upperbound - lowerbound + 1;
        var resultPage = (upperbound - 1) / resultPageSize + 1;
        var resultPageCount = (int)Math.Ceiling((double)searchResult.RowCount / resultPageSize);
        return new(leagues, resultPage, resultPageSize, resultPageCount);
    }
}
