using FluentValidation;
using iRLeagueApiCore.Server.Handlers.Results;
using iRLeagueApiCore.UnitTests.Fixtures;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Results
{
    [Collection("DbTestFixture")]
    public sealed class UploadResultHandlerTests : IClassFixture<DbTestFixture>
    {
        private readonly DbTestFixture fixture;
        private const string UploadFileName = "Data/iracing-result.json";

        public UploadResultHandlerTests(DbTestFixture fixture)
        { 
            this.fixture = fixture;
        }

        [Fact]
        public async Task Handle_ShoulNotThrow_WhenUsingTestJson()
        {
            using var stream = new FileStream(UploadFileName, FileMode.Open, FileAccess.Read);
            using var dbContext = fixture.CreateDbContext();
            var logger = Mock.Of<ILogger<UploadResultHandler>>();
            var handler = new UploadResultHandler(logger, dbContext, Array.Empty<IValidator<UploadResultRequest>>());
            var request = new UploadResultRequest(1, 1, stream, new Dictionary<int, int>());

            await handler.Handle(request, default);
        }

        [Fact]
        public async Task Handle_ShouldCreateMember_WhenMemberDoesNotExist()
        {
            using var stream = new FileStream(UploadFileName, FileMode.Open, FileAccess.Read);
            using var dbContext = fixture.CreateDbContext();
            var logger = Mock.Of<ILogger<UploadResultHandler>>();
            var handler = new UploadResultHandler(logger, dbContext, Array.Empty<IValidator<UploadResultRequest>>());
            var request = new UploadResultRequest(1, 1, stream, new Dictionary<int, int>());

            await handler.Handle(request, default);

            var newMember = await dbContext.Members
                .FirstOrDefaultAsync(x => x.IRacingId == "420");
            newMember.Should().NotBeNull();
            newMember.Firstname.Should().Be("New");
            newMember.Lastname.Should().Be("Member Guy");
        }
    }
}
