using FluentValidation;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record PostReviewToSessionRequest(long LeagueId, long SessionId, LeagueUser User, PostReviewModel Model) : IRequest<ReviewModel>;
    public class PostReviewToSessionHandler : ReviewsHandlerBase<PostReviewToSessionHandler, PostReviewToSessionRequest>, IRequestHandler<PostReviewToSessionRequest, ReviewModel>
    {
        public PostReviewToSessionHandler(ILogger<PostReviewToSessionHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PostReviewToSessionRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ReviewModel> Handle(PostReviewToSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var postReview = await CreateReviewEntityOnSession(request.LeagueId, request.SessionId, request.User, cancellationToken);
            postReview = await MapToReviewEntity(request.User, request.Model, postReview, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getReview = await MapToReviewModel(postReview.LeagueId, postReview.ReviewId, cancellationToken)
                ?? throw new InvalidOperationException("Created resource was not found");
            return getReview;
        }

        protected virtual async Task<IncidentReviewEntity> CreateReviewEntityOnSession(long leagueId, long sessionId, LeagueUser user, CancellationToken cancellationToken)
        {
            var session = await dbContext.Sessions
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SessionId == sessionId)
                .FirstOrDefaultAsync(cancellationToken)
                ?? throw new ResourceNotFoundException();
            var review = CreateVersionEntity(user, new IncidentReviewEntity());
            session.IncidentReviews.Add(review);
            return review;
        }
    }
}
