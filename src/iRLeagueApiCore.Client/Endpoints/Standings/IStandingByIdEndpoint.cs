namespace iRLeagueApiCore.Client.Endpoints.Standings;

public interface IStandingByIdEndpoint
{
    public IWithIdEndpoint<IStandingResultRowByIdEndpoint> ResultRows();
}
