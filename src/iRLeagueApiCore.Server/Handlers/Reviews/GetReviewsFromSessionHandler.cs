using FluentValidation;
using iRLeagueApiCore.Common.Models.Reviews;
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
    public record GetReviewsFromSessionRequest(long LeagueId, long SessionId) : IRequest<IEnumerable<ReviewModel>>;
    public class GetReviewsFromSessionHandler : ReviewsHandlerBase<GetReviewsFromSessionHandler, GetReviewsFromSessionRequest>,
        IRequestHandler<GetReviewsFromSessionRequest, IEnumerable<ReviewModel>>
    {
        public GetReviewsFromSessionHandler(ILogger<GetReviewsFromSessionHandler> logger, LeagueDbContext dbContext, 
            IEnumerable<IValidator<GetReviewsFromSessionRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<ReviewModel>> Handle(GetReviewsFromSessionRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getReviews = await MapToGetReviewsFromSessionAsync(request.LeagueId, request.SessionId, cancellationToken);
            return getReviews;
        }

        private async Task<IEnumerable<ReviewModel>> MapToGetReviewsFromSessionAsync(long leagueId, long sessionId, CancellationToken cancellationToken)
        {
            return await dbContext.IncidentReviews
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.SessionId == sessionId)
                .Select(MapToReviewModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
