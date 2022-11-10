using iRLeagueApiCore.Services.ResultService.Excecution;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace iRLeagueApiCore.UnitTests.Services.ResultService
{
    [Collection("DbTestFixture")]
    public class AcceptanceTests : IClassFixture<DbTestFixture>
    {
        private readonly ILogger<ExecuteEventResultCalculation> logger;
        private readonly DbTestFixture fixture;
        const int testEventId = 1;

        public AcceptanceTests(DbTestFixture fixture)
        {
            this.fixture = fixture;
            logger = Mock.Of<ILogger<ExecuteEventResultCalculation>>();
        }

        [Fact]
        public async Task ShouldCalculateResultFromDatabase()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddScoped(x => fixture.CreateDbContext());
            services.AddResultService();
            var provider = services.BuildServiceProvider();
            var sut = provider.GetRequiredService<ExecuteEventResultCalculation>();

            await sut.Execute(testEventId);
        }
    }
}
