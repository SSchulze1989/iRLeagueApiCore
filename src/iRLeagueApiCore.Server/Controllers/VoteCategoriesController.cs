using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
[Route("{leagueName}/[controller]")]
public sealed class VoteCategoriesController : LeagueApiController<VoteCategoriesController>
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

    [HttpGet]
    [AllowAnonymous]
    [Route("{id:long}")]
    public async Task<ActionResult<ReviewModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] voteCategory {ReviewId} from {LeagueName} by {UserName}", "Get", id, leagueName,
                GetUsername());
        var request = new GetVoteCategoryRequest(leagueId, id);
        var getVoteCategory = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Returning entry for voteCategory {VoteCategoryId} from {LeagueName}", getVoteCategory.Id, leagueName);
        return Ok(getVoteCategory);
    }

    [HttpPost]
    [Route("")]
    public async Task<ActionResult<VoteCategoryModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId,
        [FromBody] PostVoteCategoryModel postVoteCategory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] voteCategory to {LeagueName} by {UserName}", "Post",
            leagueName, GetUsername());
        var request = new PostVoteCategoryRequest(leagueId, postVoteCategory);
        var getVoteCategory = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return created entry for voteCategory {VoteCategoryId} from {LeagueName}", getVoteCategory.Id, leagueName);
        return CreatedAtAction(nameof(Get), new { leagueName, id = getVoteCategory.Id }, getVoteCategory);
    }

    [HttpPut]
    [Route("{id:long}")]
    public async Task<ActionResult<VoteCategoryModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        [FromBody] PutVoteCategoryModel putVoteCategory, CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] voteCategory {VoteCategoryId} from {LeagueName} by {UserName}", "Put", id, leagueName,
            GetUsername());
        var request = new PutVoteCategoryRequest(leagueId, id, putVoteCategory);
        var getVoteCategory = await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Return entry for voteCategory {VoteCategoryId} from {LeagueName}", getVoteCategory.Id, leagueName);
        return Ok(getVoteCategory);
    }

    [HttpDelete]
    [Route("{id:long}")]
    public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{Method}] voteCategory {VoteCategoryId} from {LeagueName} by {UserName}", "Delete",
            id, leagueName,
            GetUsername());
        var request = new DeleteVoteCategoryRequest(leagueId, id);
        await mediator.Send(request, cancellationToken);
        _logger.LogInformation("Deleted voteCategory {VoteCategoryId} from {LeagueName}", id, leagueName);
        return NoContent();
    }
}
