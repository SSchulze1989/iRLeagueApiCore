﻿using FluentValidation;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Exceptions;
using iRLeagueApiCore.Server.Models;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Server.Handlers.Reviews
{
    public record PutReviewRequest(long LeagueId, long ReviewId, LeagueUser User, PutReviewModel Model) : IRequest<ReviewModel>;

    public class PutReviewHandler : ReviewsHandlerBase<PutReviewHandler, PutReviewRequest>, IRequestHandler<PutReviewRequest, ReviewModel>
    {
        public PutReviewHandler(ILogger<PutReviewHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<PutReviewRequest>> validators) : 
            base(logger, dbContext, validators)
        {
        }

        public async Task<ReviewModel> Handle(PutReviewRequest request, CancellationToken cancellationToken)
        {
            await validators.ValidateAllAndThrowAsync(request, cancellationToken);
            var putReview = await GetReviewEntity(request.LeagueId, request.ReviewId, cancellationToken)
                ?? throw new ResourceNotFoundException();
            putReview = await MapToReviewEntity(request.User, request.Model, putReview, cancellationToken);
            await dbContext.SaveChangesAsync(cancellationToken);
            var getReview = await MapToReviewModel(putReview.LeagueId, putReview.ReviewId, cancellationToken);
            return getReview;
        }
    }
}