using iRLeagueApiCore.Common.Models.Standings;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Standings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers;

[Route("/{leagueName}/[controller]")]
[TypeFilter(typeof(LeagueAuthorizeAttribute))]
[TypeFilter(typeof(InsertLeagueIdAttribute))]
[RequireLeagueRole]
public sealed class StandingsController : LeagueApiController<StandingsController>
{
    public StandingsController(ILogger<StandingsController> logger, IMediator mediator) :
        base(logger, mediator)
    {
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("/{leagueName}/Seasons/{seasonId:long}/[controller]")]
    public async Task<ActionResult<IEnumerable<StandingsModel>>> GetFromSeason([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long seasonId,
        CancellationToken cancellationToken = default)
    {
        var request = new GetStandingsFromSeasonRequest(leagueId, seasonId);
        var getStandings = await mediator.Send(request, cancellationToken);
        return Ok(getStandings);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("/{leagueName}/Events/{eventId:long}/[controller]")]
    public async Task<ActionResult<IEnumerable<StandingsModel>>> GetFromEvent([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long eventId,
        CancellationToken cancellationToken = default)
    {
        var request = new GetStandingsFromEventRequest(leagueId, eventId);
        var getStandings = await mediator.Send(request, cancellationToken);
        return Ok(getStandings);
    }
}
