using FluentValidation;
using iRLeagueApiCore.Common.Models.Members;
using iRLeagueApiCore.Common.Models.Reviews;
using iRLeagueApiCore.Server.Handlers.Reviews;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Reviews
{
    [Collection("DbTestFixture")]
    public class PostReviewToSessionDbTestFixture : HandlersTestsBase<PostReviewToSessionHandler, PostReviewToSessionRequest, ReviewModel>
    {
        public PostReviewToSessionDbTestFixture(DbTestFixture fixture) : base(fixture)
        {
        }

        public static PostReviewModel TestReviewModel => new PostReviewModel()
        {
            FullDescription = "Full Description",
            Corner = "1",
            OnLap = "2",
            IncidentNr = "3.4",
            IncidentKind = "Unfall",
            TimeStamp = System.TimeSpan.FromMinutes(1.2),
            InvolvedMembers = new MemberInfoModel[]
            {
                new MemberInfoModel() { MemberId = testMemberId},
                new MemberInfoModel() { MemberId = testMemberId + 1},
            },
            VoteResults = new VoteModel[0],
        };

        protected override PostReviewToSessionHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PostReviewToSessionRequest> validator)
        {
            return new PostReviewToSessionHandler(logger, dbContext, new[] { validator });
        }

        protected virtual PostReviewToSessionRequest DefaultRequest(long leagueId, long reviewId)
        {

            return new PostReviewToSessionRequest(leagueId, reviewId, DefaultUser(), TestReviewModel);
        }

        protected override PostReviewToSessionRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testReviewId);
        }

        protected override void DefaultAssertions(PostReviewToSessionRequest request, ReviewModel result, LeagueDbContext dbContext)
        {
            var expected = TestReviewModel;
            result.LeagueId.Should().Be(request.LeagueId);
            result.SessionId.Should().Be(request.SessionId);
            result.ReviewId.Should().NotBe(0);
            result.Corner.Should().Be(expected.Corner);
            result.OnLap.Should().Be(expected.OnLap);
            result.IncidentKind.Should().Be(expected.IncidentKind);
            result.IncidentNr.Should().Be(expected.IncidentNr);
            result.TimeStamp.Should().Be(expected.TimeStamp);
            result.InvolvedMembers.Should().HaveSameCount(expected.InvolvedMembers);
            var reviewEntity = dbContext.IncidentReviews
                .SingleOrDefault(x => x.ReviewId == result.ReviewId);
            reviewEntity.Should().NotBeNull();
            foreach ((var member, var expectedMember) in result.InvolvedMembers.OrderBy(x => x.MemberId).Zip(reviewEntity.InvolvedMembers.OrderBy(x => x.Id)))
            {
                AssertMemberInfo(expectedMember, member);
            }
            AssertCreated(request.User, DateTime.UtcNow, result);
            base.DefaultAssertions(request, result, dbContext);
        }

        private void AssertMemberInfo(MemberEntity expected, MemberInfoModel result)
        {
            result.MemberId.Should().Be(expected.Id);
            result.FirstName.Should().Be(expected.Firstname);
            result.LastName.Should().Be(expected.Lastname);
        }

        [Fact]
        public override async Task<ReviewModel> ShouldHandleDefault()
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
