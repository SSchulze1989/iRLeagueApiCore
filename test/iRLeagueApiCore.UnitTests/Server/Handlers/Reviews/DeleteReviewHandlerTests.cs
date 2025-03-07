﻿using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews;

public sealed class DeleteReviewHandlerTests : ReviewsHandlersTestsBase<DeleteReviewHandler, DeleteReviewRequest, Unit>
{
    public DeleteReviewHandlerTests() : base()
    {
    }

    protected override DeleteReviewHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteReviewRequest> validator)
    {
        return new DeleteReviewHandler(logger, dbContext, new[] { validator }, mockResultCalculationQueue);
    }

    private DeleteReviewRequest DefaultRequest(long reviewId)
    {

        return new DeleteReviewRequest(reviewId);
    }

    protected override DeleteReviewRequest DefaultRequest()
    {
        return DefaultRequest(TestReviewId);
    }

    protected override void DefaultAssertions(DeleteReviewRequest request, Unit result, LeagueDbContext dbContext)
    {
        var deletedReview = dbContext.IncidentReviews
            .SingleOrDefault(x => x.ReviewId == request.ReviewId);
        deletedReview.Should().BeNull();
        base.DefaultAssertions(request, result, dbContext);
    }

    [Fact]
    public override async Task ShouldHandleDefault()
    {
        await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(0L, defaultId)]
    [InlineData(defaultId, 0L)]
    [InlineData(-42L, defaultId)]
    [InlineData(defaultId, -42L)]
    public async Task ShouldHandleNotFoundAsync(long? leagueId, long? reviewId)
    {
        leagueId ??= TestLeagueId;
        reviewId ??= TestReviewId;
        accessMockHelper.SetCurrentLeague(leagueId.Value);
        var request = DefaultRequest(reviewId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
