using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Services.ResultService.Excecution;

namespace iRLeagueApiCore.Server.Handlers.Reviews;

public record GetReviewRequest(long ReviewId, bool IncludeComments) : IRequest<ReviewModel>;

public sealed class GetReviewHandler : ReviewsHandlerBase<GetReviewHandler, GetReviewRequest, ReviewModel>
{
    public GetReviewHandler(ILogger<GetReviewHandler> logger, LeagueDbContext dbContext, IEnumerable<IValidator<GetReviewRequest>> validators,
        IResultCalculationQueue resultCalculationQueue) : base(logger, dbContext, validators, resultCalculationQueue)
    {
    }

    public override async Task<ReviewModel> Handle(GetReviewRequest request, CancellationToken cancellationToken)
    {
        await validators.ValidateAllAndThrowAsync(request, cancellationToken);
        var getReview = await MapToReviewModel(request.ReviewId, request.IncludeComments, cancellationToken)
            ?? throw new ResourceNotFoundException();
        return getReview;
    }
}
