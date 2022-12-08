using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.Tests.ResultService.DataAcess;

public abstract class DataAccessTestsBase : IAsyncLifetime
{
    protected readonly Fixture fixture;
    protected readonly DataAccessMockHelper accessMockHelper;
    protected readonly LeagueDbContext dbContext;

    public DataAccessTestsBase()
    {
        fixture = new Fixture();
        accessMockHelper = new();
        dbContext = accessMockHelper.CreateMockDbContext();
        fixture.Register(() => dbContext);
    }

    public async Task InitializeAsync()
    {
        await accessMockHelper.PopulateBasicTestSet(dbContext);
    }

    public async Task DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }
}
