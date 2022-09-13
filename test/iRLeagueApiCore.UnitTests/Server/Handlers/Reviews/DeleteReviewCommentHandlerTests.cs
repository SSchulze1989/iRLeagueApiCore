using FluentAssertions;
using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews
{
    [Collection("HandlerTests")]
    public class DeleteReviewCommentHandlerTests : HandlersTestsBase<DeleteReviewCommentHandler, DeleteReviewCommentRequest, Unit>
    {
        public DeleteReviewCommentHandlerTests(DbTestFixture fixture) : base(fixture)
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

        protected override void DefaultAssertions(DeleteReviewCommentRequest request, Unit result, LeagueDbContext dbContext)
        {
            var deletedComment = dbContext.ReviewComments
                .SingleOrDefault(x => x.CommentId == request.CommentId);
            deletedComment.Should().BeNull();
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
}
