using FluentValidation;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record DeleteReviewRequest(long LeagueId, long ReviewId) : IRequest;

    public class DeleteReviewHandler : ReviewsHandlerBase<DeleteReviewHandler, DeleteReviewRequest>, 
        IRequestHandler<DeleteReviewRequest>
    {
        public DeleteReviewHandler(ILogger<DeleteReviewHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<DeleteReviewRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<Unit> Handle(DeleteReviewRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var deleteReview = await GetReviewEntity(request.LeagueId, request.ReviewId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            dbContext.Remove(deleteReview);
            await dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
