using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Mocking.DataAccess;

public abstract class DataAccessTestsBase : IAsyncLifetime
{
    protected readonly string databaseName;
    protected readonly Fixture fixture;
    protected readonly DataAccessMockHelper accessMockHelper;
    protected readonly LeagueDbContext dbContext;

    public DataAccessTestsBase()
    {
        fixture = new Fixture();
        accessMockHelper = new();
        databaseName = fixture.Create<string>();
        dbContext = accessMockHelper.CreateMockDbContext(databaseName);
        fixture.Register(() => dbContext);
    }

    public virtual async Task InitializeAsync()
    {
        dbContext.Database.EnsureCreated();
        await accessMockHelper.PopulateBasicTestSet(dbContext);
    }

    public virtual async Task DisposeAsync()
    {
        dbContext.Database.EnsureDeleted();
        await dbContext.DisposeAsync();
    }
}
