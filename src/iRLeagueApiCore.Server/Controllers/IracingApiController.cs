using Aydsko.iRacingData;
using Aydsko.iRacingData.Exceptions;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Common.Models.Leagues;
using iRLeagueApiCore.Common.Responses;
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
    [Route("Leagues/Searchs")]
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

    /// <summary>
    /// Test if 
    /// </summary>
    /// <param name="iracingLeagueId"></param>
    /// <param name="cancellationToken"></param>
    /// <param name="dataClient"></param>
    /// <returns></returns>
    [HttpPost]
    [Authorize]
    [Route("League/{iracingLeagueId:int}/Test")]
    public async Task<ActionResult<StatusResponse>> TestLeagueConnection([FromRoute] int iracingLeagueId, [FromServices] IDataClient dataClient, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await dataClient.GetLeagueAsync(iracingLeagueId, cancellationToken: cancellationToken);
        } 
        catch (HttpRequestException)
        {
            return Ok(new StatusResponse()
            {
                Status = "Not Found",
                Message = $"No league found with id {iracingLeagueId}",
            });
        }
        catch (iRacingForbiddenResponseException)
        {
            return Ok(new StatusResponse()
            {
                Status = "Access Denied",
                Message = "Cannot access private league data",
            });
        }
        return Ok(new StatusResponse()
        {
            Status = "Success",
            Message = "League connection established successfully",
        });
    }
}
