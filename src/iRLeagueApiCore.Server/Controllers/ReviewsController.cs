﻿using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Authentication;
using iRLeagueApiCore.Server.Filters;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.Server.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Controllers
{
    [ApiController]
    [TypeFilter(typeof(LeagueAuthorizeAttribute))]
    [TypeFilter(typeof(InsertLeagueIdAttribute))]
    [TypeFilter(typeof(DefaultExceptionFilterAttribute))]
    [RequireLeagueRole(LeagueRoles.Admin, LeagueRoles.Steward)]
    [Route("{leagueName}/[controller]")]
    public class ReviewsController : LeagueApiController
    {
        private readonly ILogger<ReviewsController> _logger;
        private readonly IMediator mediator;

        public ReviewsController(ILogger<ReviewsController> logger, IMediator mediator)
        {
            _logger = logger;
            this.mediator = mediator;
        }

        [HttpGet]
        [Route("{id:long}")]
        public async Task<ActionResult<ReviewModel>> Get([FromRoute] string leagueName, [FromFilter] long leagueId, [FromRoute] long id, 
            CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] review {ReviewId} from {LeagueName} by {UserName}", "Get", id, leagueName,
                    User.Identity.Name);
            var request = new GetReviewRequest(leagueId, id);
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
                sessionId, leagueName, User.Identity.Name);
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
                User.Identity.Name);
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
                User.Identity.Name);
            var request = new DeleteReviewRequest(leagueId, id);
            await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Deleted review {ReviewId} from {LeagueName}", id, leagueName);
            return NoContent();
        }

        [HttpGet]
        [Route("/{leagueName}/Sessions/{sessionId:long}/Reviews")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetFromSession([FromRoute] string leagueName, [FromFilter] long leagueId, 
            [FromRoute] long sessionId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] all reviews on session {SessionId} from {LeagueName} by {UserName}", "Get",
                sessionId, leagueName, User.Identity.Name);
            var request = new GetReviewsFromSessionRequest(leagueId, sessionId);
            var getReviews = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for reviews on session {SessionId} from {LeagueName}",
                getReviews.Count(), sessionId, leagueName);
            return Ok(getReviews);
        }

        [HttpGet]
        [Route("/{leagueName}/Events/{eventId:long}/Reviews")]
        public async Task<ActionResult<IEnumerable<ReviewModel>>> GetFromEvent([FromRoute] string leagueName, [FromFilter] long leagueId,
            [FromRoute] long eventId, CancellationToken cancellationToken)
        {
            _logger.LogInformation("[{Method}] all reviews on event {EventId} from {LeagueName} by {UserName}", "Get",
                eventId, leagueName, User.Identity.Name);
            var request = new GetReviewsFromEventRequest(leagueId, eventId);
            var getReviews = await mediator.Send(request, cancellationToken);
            _logger.LogInformation("Return {Count} entries for reviews on event {EventId} from {LeagueName}",
                getReviews.Count(), eventId, leagueName);
            return Ok(getReviews);
        }
    }
}