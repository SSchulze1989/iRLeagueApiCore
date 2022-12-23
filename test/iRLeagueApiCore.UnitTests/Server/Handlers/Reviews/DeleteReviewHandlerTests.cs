using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews;

[Collection("DbTestFixture")]
public class DeleteReviewDbTestFixture : HandlersTestsBase<DeleteReviewHandler, DeleteReviewRequest, Unit>
{
    public DeleteReviewDbTestFixture(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override DeleteReviewHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteReviewRequest> validator)
    {
        return new DeleteReviewHandler(logger, dbContext, new[] { validator });
    }

    protected virtual DeleteReviewRequest DefaultRequest(long leagueId, long reviewId)
    {

        return new DeleteReviewRequest(leagueId, reviewId);
    }

    protected override DeleteReviewRequest DefaultRequest()
    {
        return DefaultRequest(testLeagueId, testReviewId);
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
    [InlineData(0, testReviewId)]
    [InlineData(testLeagueId, 0)]
    [InlineData(42, testReviewId)]
    [InlineData(testLeagueId, 42)]
    public async Task ShouldHandleNotFoundAsync(long leagueId, long resultConfigId)
    {
        var request = DefaultRequest(leagueId, resultConfigId);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
