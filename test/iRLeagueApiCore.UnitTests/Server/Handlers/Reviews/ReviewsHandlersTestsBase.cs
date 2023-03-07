using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews;
public abstract class ReviewsHandlersTestsBase<THandler, TRequest, TResult> :
    HandlersTestsBase<THandler, TRequest, TResult>
    where THandler : IRequestHandler<TRequest, TResult>
    where TRequest : class, IRequest<TResult>
{
    public override async Task InitializeAsync()
    {
        await base.InitializeAsync();
        var @event = dbContext.Events.First();
        var reviews = accessMockHelper.CreateReviews(@event);
        dbContext.IncidentReviews.RemoveRange(dbContext.IncidentReviews);
        dbContext.IncidentReviews.AddRange(reviews);
        foreach(var review in reviews)
        {
            review.Comments = accessMockHelper.CreateComments(review).ToList();
            dbContext.ReviewComments.AddRange(review.Comments);
            review.AcceptedReviewVotes = accessMockHelper.AcceptedReviewVoteBuilder()
                .CreateMany()
                .ToList();
            dbContext.AcceptedReviewVotes.AddRange(review.AcceptedReviewVotes);
        }
        await dbContext.SaveChangesAsync();
    }
}
