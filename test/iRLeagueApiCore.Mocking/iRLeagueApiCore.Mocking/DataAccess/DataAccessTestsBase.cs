using iRLeagueDatabaseCore;
using iRLeagueDatabaseCore.Models;
using Microsoft.EntityFrameworkCore;

namespace iRLeagueApiCore.Mocking.DataAccess;

public abstract class DataAccessTestsBase : IAsyncLifetime
{
    protected readonly string databaseName;
    protected readonly Fixture fixture;
    protected readonly DataAccessMockHelper accessMockHelper;
    protected readonly LeagueDbContext dbContext;
    
    protected ILeagueProvider LeagueProvider => accessMockHelper.LeagueProvider;

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
        accessMockHelper.SetCurrentLeague(await dbContext.Leagues.FirstAsync());
    }

    public virtual async Task DisposeAsync()
    {
        dbContext.Database.EnsureDeleted();
        await dbContext.DisposeAsync();
    }
}
