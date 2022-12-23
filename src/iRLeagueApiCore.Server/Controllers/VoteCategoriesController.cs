using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Reviews;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
[Route("{leagueName}/[controller]")]
public class VoteCategoriesController : LeagueApiController<VoteCategoriesController>
{
    public VoteCategoriesController(ILogger<VoteCategoriesController> logger, IMediator mediator) :
        base(logger, mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("")]
    public async Task<ActionResult<IEnumerable<VoteCategoryModel>>> GetAll([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
            CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] all vote categories from {LeagueName} by {UserName}", "Get", leagueName,
                GetUsername());
        var request = new GetLeagueVoteCategoriesRequest(leagueId);
        var getVoteCategories = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Returning entry for vote categories from {LeagueName}", leagueName);
        return Ok(getVoteCategories);
    }
}
