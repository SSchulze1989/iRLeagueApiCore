using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.Server.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace iRLeagueApiCore.Server.Controllers
{
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
    [Route("{leagueName}/[controller]")]
    public class ReviewsController : LeagueApiController<ReviewsController>
    {
        public ReviewsController(ILogger<ReviewsController> logger, IMediator mediator) : base(logger, mediator)
        {
        }

        private static bool IncludeComments(LeagueUser user)
        {
            return user.IsInRole(LeagueRoles.Admin, LeagueRoles.Steward);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("{id:long}")]
        public async Task<ActionResult<ReviewModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] review {ReviewId} from {LeagueName} by {UserName}", "Get", id, leagueName,
                    GetUsername());
            var includeComments = IncludeComments(new LeagueUser(leagueName, User));
            var request = new GetReviewRequest(leagueId, id, includeComments);
            var getReview = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Returning entry for review {ReviewId} from {LeagueName}", getReview.ReviewId, leagueName);
            return Ok(getReview);
        }

        [HttpPost]
        [Route("/{leagueName}/Sessions/{sessionId:long}/[controller]")]
        public async Task<ActionResult<ReviewModel>> Post([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long sessionId,
            PostReviewModel postReview, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] review to session {SessionId} from {LeagueName} by {UserName}", "Post",
                sessionId, leagueName, GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PostReviewToSessionRequest(leagueId, sessionId, leagueUser, postReview);
            var getReview = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return created entry for review {ReviewId} from {LeagueName}", getReview.ReviewId, leagueName);
            return CreatedAtAction(nameof(Get), new { leagueName, id = getReview.ReviewId }, getReview);
        }

        [HttpPut]
        [Route("{id:long}")]
        public async Task<ActionResult<ReviewModel>> Put([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
            PutReviewModel putReview, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] review {ReviewId} from {LeagueName} by {UserName}", "Put", id, leagueName,
                GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new PutReviewRequest(leagueId, id, leagueUser, putReview);
            var getReview = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for review {ReviewId} from {LeagueName}", getReview.ReviewId, leagueName);
            return Ok(getReview);
        }

        [HttpDelete]
        [Route("{id:long}")]
        public async Task<ActionResult> Delete([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] review {ReviewId} from {LeagueName} by {UserName}", "Delete",
                id, leagueName,
                GetUsername());
            var request = new DeleteReviewRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted review {ReviewId} from {LeagueName}", id, leagueName);
            return NoContent();
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}/Sessions/{sessionId:long}/Reviews")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetFromSession([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long sessionId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] all reviews on session {SessionId} from {LeagueName} by {UserName}", "Get",
                sessionId, leagueName, GetUsername());
            var includeComments = IncludeComments(new LeagueUser(leagueName, User));
            var request = new GetReviewsFromSessionRequest(leagueId, sessionId, includeComments);
            var getReviews = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for reviews on session {SessionId} from {LeagueName}",
                getReviews.Count(), sessionId, leagueName);
            return Ok(getReviews);
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("/{leagueName}/Events/{eventId:long}/Reviews")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetFromEvent([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long eventId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] all reviews on event {EventId} from {LeagueName} by {UserName}", "Get",
                eventId, leagueName, GetUsername());
            var includeComments = IncludeComments(new LeagueUser(leagueName, User));
            var request = new GetReviewsFromEventRequest(leagueId, eventId, includeComments);
            var getReviews = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for reviews on event {EventId} from {LeagueName}",
                getReviews.Count(), eventId, leagueName);
            return Ok(getReviews);
        }

        [HttpPost]
        [Route("{id:long}/MoveToSession/{sessionId:long}")]
        public async Task<ActionResult<ReviewModel>> MoveReviewToSession([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id,
            [FromRoute] long sessionId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] Move review {ReviewId} to event {EventId} from {LeagueName} by {UserName}", "Post",
                id, sessionId, leagueName, GetUsername());
            var leagueUser = new LeagueUser(leagueName, User);
            var request = new MoveReviewToSessionRequest(leagueId, sessionId, id, leagueUser);
            var getReview = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return entry for review {ReviewId} from {LeagueName}", getReview.ReviewId, leagueName);
            return Ok(getReview);
        }
    }
}
