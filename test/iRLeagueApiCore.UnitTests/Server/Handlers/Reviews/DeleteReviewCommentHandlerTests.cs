using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews;

[Collection("DbTestFixture")]
public class DeleteReviewCommentDbTestFixture : HandlersTestsBase<DeleteReviewCommentHandler, DeleteReviewCommentRequest, Unit>
{
    public DeleteReviewCommentDbTestFixture(DbTestFixture fixture) : base(fixture)
    {
    }

    protected override DeleteReviewCommentHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteReviewCommentRequest> validator)
    {
        return new DeleteReviewCommentHandler(logger, dbContext, new[] { validator });
    }

    protected virtual DeleteReviewCommentRequest DefaultRequest(long leagueId, long commentId)
    {

        return new DeleteReviewCommentRequest(leagueId, commentId);
    }

    protected override DeleteReviewCommentRequest DefaultRequest()
    {
        return DefaultRequest(testLeagueId, testCommentId);
    }

    protected override void DefaultPreTestAssertions(DeleteReviewCommentRequest request, LeagueDbContext dbContext)
    {
        dbContext.ReviewComments.Should().Contain(x => x.CommentId == request.CommentId);
        base.DefaultPreTestAssertions(request, dbContext);
    }

    protected override void DefaultAssertions(DeleteReviewCommentRequest request, Unit result, LeagueDbContext dbContext)
    {
        dbContext.ReviewComments.Should().NotContain(x => x.CommentId == request.CommentId);
        base.DefaultAssertions(request, result, dbContext);
    }

    private void AssertMemberInfo(MemberInfoModel expected, MemberInfoModel result)
    {
        result.MemberId.Should().Be(expected.MemberId);
    }

    [Fact]
    public override async Task<Unit> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Theory]
    [InlineData(0, testCommentId)]
    [InlineData(testLeagueId, 0)]
    [InlineData(42, testCommentId)]
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
