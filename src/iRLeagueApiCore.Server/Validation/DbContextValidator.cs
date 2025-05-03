namespace iRLeagueApiCore.Server.Validation;

public abstract class DbContextValidator<T> : AbstractValidator<T>
{
    protected ILeagueDbContext dbContext;

    protected DbContextValidator(ILeagueDbContext dbContext)
    {
        this.dbContext = dbContext;
    }
}
