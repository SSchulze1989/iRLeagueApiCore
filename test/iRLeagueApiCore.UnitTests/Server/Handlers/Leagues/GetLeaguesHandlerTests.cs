using FluentValidation;
using iRLeagueApiCore.Common.Models;
using iRLeagueApiCore.Server.Handlers.Leagues;
using iRLeagueApiCore.UnitTests.Fixtures;
using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.UnitTests.Server.Handlers.Leagues;

[Collection("DbTestFixture")]
public sealed class GetLeaguesDbTestFixture : HandlersTestsBase<GetLeaguesHandler, GetLeaguesRequest, IEnumerable<LeagueModel>>
{
    public GetLeaguesDbTestFixture() : base()
    {
    }

    protected override GetLeaguesHandler CreateTestHandler(LeagueDbContext dbContext, IValidator<GetLeaguesRequest> validator)
    {
        return new GetLeaguesHandler(logger, dbContext, new IValidator<GetLeaguesRequest>[] { validator });
    }

    protected override GetLeaguesRequest DefaultRequest()
    {
        return new GetLeaguesRequest();
    }

    [Fact]
    public override async Task<IEnumerable<LeagueModel>> ShouldHandleDefault()
    {
        return await base.ShouldHandleDefault();
    }

    [Fact]
    public override async Task ShouldHandleValidationFailed()
    {
        await base.ShouldHandleValidationFailed();
    }
}
