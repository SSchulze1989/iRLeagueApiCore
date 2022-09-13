using FluentAssertions;
using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews
{
    [Collection("HandlerTests")]
    public class PostReviewCommentToReviewHandlerTests : HandlersTestsBase<PostReviewCommentToReviewHandler, PostReviewCommentToReviewRequest, ReviewCommentModel>
    {
        public PostReviewCommentToReviewHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        private static PutReviewCommentModel TestReviewComment => new PutReviewCommentModel()
        {
            Text = "Test Comment",
            Votes = new[] { new CommentVoteModel()
            {
                Description = "Test Vote",
                MemberAtFault = new MemberInfoModel() { MemberId = testMemberId},
            } },
        };

        protected override PostReviewCommentToReviewHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostReviewCommentToReviewRequest> validator)
        {
            return new PostReviewCommentToReviewHandler(logger, dbContext, new[] { validator });
        }

        protected virtual PostReviewCommentToReviewRequest DefaultRequest(long leagueId, long reviewId)
        {

            return new PostReviewCommentToReviewRequest(leagueId, reviewId, DefaultUser(), TestReviewComment);
        }

        protected override PostReviewCommentToReviewRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testReviewId);
        }

        protected override void DefaultAssertions(PostReviewCommentToReviewRequest request, ReviewCommentModel result, LeagueDbContext dbContext)
        {
            var expected = request.Model;
            result.LeagueId.Should().Be(result.LeagueId);
            result.ReviewId.Should().NotBe(0);
            result.Text.Should().Be(expected.Text);
            result.Votes.Should().HaveSameCount(expected.Votes);
            foreach ((var vote, var expectedVote) in result.Votes.Zip(expected.Votes))
            {
                AssertCommentVote(expectedVote, vote);
            }
            AssertCreated(request.User, DateTime.UtcNow, result);
            base.DefaultAssertions(request, result, dbContext);
        }

        private void AssertCommentVote(CommentVoteModel expected, CommentVoteModel result)
        {
            result.Description.Should().Be(expected.Description);
            AssertMemberInfo(expected.MemberAtFault, result.MemberAtFault);
        }

        private void AssertMemberInfo(MemberInfoModel expected, MemberInfoModel result)
        {
            result.MemberId.Should().Be(expected.MemberId);
        }

        [Fact]
        public override async Task<ReviewCommentModel> ShouldHandleDefault()
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
}
