using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Mocking.DataAccess;

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

    public virtual async Task InitializeAsync()
    {
        await accessMockHelper.PopulateBasicTestSet(dbContext);
    }

    public virtual async Task DisposeAsync()
    {
        await dbContext.DisposeAsync();
    }
}
