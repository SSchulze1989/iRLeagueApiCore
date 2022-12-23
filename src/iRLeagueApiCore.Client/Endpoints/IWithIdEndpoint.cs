namespace iRLeagueApiCore.Client.Endpoints;

public interface IWithIdEndpoint<T>
{
    T WithId(long id);
}
