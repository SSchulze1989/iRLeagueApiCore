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
    public class PutReviewCommentHandlerTests : HandlersTestsBase<PutReviewCommentHandler, PutReviewCommentRequest, ReviewCommentModel>
    {
        public PutReviewCommentHandlerTests(DbTestFixture fixture) : base(fixture)
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

        protected override PutReviewCommentHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutReviewCommentRequest> validator)
        {
            return new PutReviewCommentHandler(logger, dbContext, new[] { validator });
        }

        protected virtual PutReviewCommentRequest DefaultRequest(long leagueId, long commentId)
        {

            return new PutReviewCommentRequest(leagueId, commentId, DefaultUser(), TestReviewComment);
        }

        protected override PutReviewCommentRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testCommentId);
        }

        protected override void DefaultAssertions(PutReviewCommentRequest request, ReviewCommentModel result, LeagueDbContext dbContext)
        {
            var expected = request.Model;
            result.LeagueId.Should().Be(request.LeagueId);
            result.CommentId.Should().Be(request.CommentId);
            result.Text.Should().Be(expected.Text);
            result.Votes.Should().HaveSameCount(expected.Votes);
            foreach ((var vote, var expectedVote) in result.Votes.Zip(expected.Votes))
            {
                AssertCommentVote(expectedVote, vote);
            }
            AssertChanged(request.User, DateTime.UtcNow, result);
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
