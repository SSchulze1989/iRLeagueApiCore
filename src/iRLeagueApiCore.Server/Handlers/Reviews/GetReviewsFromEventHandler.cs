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
    public record GetReviewsFromEventRequest(long LeagueId, long EventId) : IRequest<IEnumerable<ReviewModel>>;
    public class GetReviewsFromEventHandler : ReviewsHandlerBase<GetReviewsFromEventHandler, GetReviewsFromEventRequest>,
        IRequestHandler<GetReviewsFromEventRequest, IEnumerable<ReviewModel>>
    {
        public GetReviewsFromEventHandler(ILogger<GetReviewsFromEventHandler> logger, LeagueDbContext dbContext,
            IEnumerable<IValidator<GetReviewsFromEventRequest>> validators) : base(logger, dbContext, validators)
        {
        }

        public async Task<IEnumerable<ReviewModel>> Handle(GetReviewsFromEventRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getReviews = await MapToGetReviewsFromEventAsync(request.LeagueId, request.EventId, cancellationToken);
            return getReviews;
        }

        private async Task<IEnumerable<ReviewModel>> MapToGetReviewsFromEventAsync(long leagueId, long EventId, CancellationToken cancellationToken)
        {
            return await dbContext.IncidentReviews
                .Where(x => x.LeagueId == leagueId)
                .Where(x => x.Session.EventId == EventId)
                .Select(MapToReviewModelExpression)
                .ToListAsync(cancellationToken);
        }
    }
}
