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
    public class PutReviewHandlerTests : HandlersTestsBase<PutReviewHandler, PutReviewRequest, ReviewModel>
    {
        public PutReviewHandlerTests(DbTestFixture fixture) : base(fixture)
        {
        }

        public static PutReviewModel TestReviewModel => new PutReviewModel()
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
        };

        protected override PutReviewHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<PutReviewRequest> validator)
        {
            return new PutReviewHandler(logger, dbContext, new[] { validator });
        }

        protected virtual PutReviewRequest DefaultRequest(long leagueId, long reviewId)
        {
            
            return new PutReviewRequest(leagueId, reviewId, DefaultUser(), TestReviewModel);
        }

        protected override PutReviewRequest DefaultRequest()
        {
            return DefaultRequest(testLeagueId, testReviewId);
        }

        protected override void DefaultAssertions(PutReviewRequest request, ReviewModel result, LeagueDbContext dbContext)
        {
            var expected = request.Model;
            result.LeagueId.Should().Be(request.LeagueId);
            result.ReviewId.Should().Be(request.ReviewId);
            result.Corner.Should().Be(expected.Corner);
            result.OnLap.Should().Be(expected.OnLap);
            result.IncidentKind.Should().Be(expected.IncidentKind);
            result.IncidentNr.Should().Be(expected.IncidentNr);
            result.TimeStamp.Should().Be(expected.TimeStamp);
            result.InvolvedMembers.Should().HaveSameCount(expected.InvolvedMembers);
            foreach((var member, var expectedMember) in result.InvolvedMembers.Zip(expected.InvolvedMembers))
            {
                AssertMemberInfo(expectedMember, member);
            }
            AssertChanged(request.User, DateTime.UtcNow, result);
            base.DefaultAssertions(request, result, dbContext);
        }

        private void AssertMemberInfo(MemberInfoModel expected, MemberInfoModel result)
        {
            result.MemberId.Should().Be(expected.MemberId);
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
