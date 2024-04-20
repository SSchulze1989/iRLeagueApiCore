
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Leagues;
using iRLeagueApiCore.Server.Handlers.Leagues;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[Route("[controller]")]
public class IracingApiController : LeagueApiController<IracingApiController>
{
    public IracingApiController(ILogger<IracingApiController> logger, IMediator mediator) : base(logger, mediator)
    {
    }

    [HttpPost]
    [Authorize]
    [Route("SearchLeagues")]
    public async Task<ActionResult<PaginatedResultModel<IEnumerable<SearchIracingLeagueResultModel>>>> SearchLeagues([FromBody] PostSearchIracingLeaguesModel searchModel,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 40,
        CancellationToken cancellationToken = default)
    {
        if (page <= 0)
        {
            page = 1;
        }
        var request = new SearchIracingLeaguesRequest(searchModel.Search, page, pageSize);
        var result = await mediator.Send(request, cancellationToken);
        return Ok(result);
    }
}
