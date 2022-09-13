using FluentValidation;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record GetReviewRequest(long LeagueId, long ReviewId) : IRequest<ReviewModel>;

    public class GetReviewHandler : ReviewsHandlerBase<GetReviewHandler, GetReviewRequest>, IRequestHandler<GetReviewRequest, ReviewModel>
    {
        public GetReviewHandler(ILogger<GetReviewHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetReviewRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ReviewModel> Handle(GetReviewRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var getReview = await MapToReviewModel(request.LeagueId, request.ReviewId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            return getReview;
        }
    }
}
