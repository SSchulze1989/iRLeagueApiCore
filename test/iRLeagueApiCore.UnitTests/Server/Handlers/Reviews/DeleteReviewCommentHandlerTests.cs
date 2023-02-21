using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews;

[Collection("DbTestFixture")]
public sealed class DeleteReviewCommentHandlerTests : ReviewsHandlersTestsBase<DeleteReviewCommentHandler, DeleteReviewCommentRequest, Unit>
{
    public DeleteReviewCommentHandlerTests() : base()
    {
    }

    protected override DeleteReviewCommentHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<DeleteReviewCommentRequest> validator)
    {
        return new DeleteReviewCommentHandler(logger, dbContext, new[] { validator });
    }

    private DeleteReviewCommentRequest DefaultRequest(long leagueId, long commentId)
    {

        return new DeleteReviewCommentRequest(leagueId, commentId);
    }

    protected override DeleteReviewCommentRequest DefaultRequest()
    {
        return DefaultRequest(TestLeagueId, TestCommentId);
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
    [InlineData(0, defaultId)]
    [InlineData(defaultId, 0)]
    [InlineData(42, defaultId)]
    [InlineData(defaultId, 42)]
    public async Task ShouldHandleNotFoundAsync(long? leagueId, long? commentId)
    {
        leagueId ??= TestLeagueId;
        commentId ??= TestCommentId;
        var request = DefaultRequest(leagueId.Value, commentId.Value);
        await HandleNotFoundRequestAsync(request);
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
