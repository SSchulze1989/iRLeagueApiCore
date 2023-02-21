﻿using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews;

[Collection("DbTestFixture")]
public sealed class DeleteReviewHandlerTests : ReviewsHandlersTestsBase<DeleteReviewHandler, DeleteReviewRequest, Unit>
{
    public DeleteReviewHandlerTests() : base()
    {
    }

    protected override DeleteReviewHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteReviewRequest> validator)
    {
        return new DeleteReviewHandler(logger, dbContext, new[] { validator });
    }

    private DeleteReviewRequest DefaultRequest(long leagueId, long reviewId)
    {

        return new DeleteReviewRequest(leagueId, reviewId);
    }

    protected override DeleteReviewRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestReviewId);
    }

    protected override void DefaultAssertions(DeleteReviewRequest request, Unit result, LeagueDbContext dbContext)
    {
        var deletedReview = dbContext.IncidentReviews
            .SingleOrDefault(x => x.ReviewId == request.ReviewId);
        deletedReview.Should().BeNull();
        base.DefaultAssertions(request, result, dbContext);
    }

    private void AssertMemberInfo(MemberEntity expected, MemberInfoModel result)
    {
        result.MemberId.Should().Be(expected.Id);
        result.FirstName.Should().Be(expected.Firstname);
        result.LastName.Should().Be(expected.Lastname);
    }

    [Fact]
    public override async Task<Unit> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(0, defaultId)]
    [InlineData(defaultId, 0)]
    [InlineData(42, defaultId)]
    [InlineData(defaultId, 42)]
    public async Task ShouldHandleNotFoundAsync(long? leagueId, long? reviewId)
    {
        leagueId ??= TestLeagueId;
        reviewId ??= TestReviewId;
        var request = DefaultRequest(leagueId.Value, reviewId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
