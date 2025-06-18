namespace iRLeagueApiCore.Client.Endpoints;

public interface IPostGetAllEndpoint<TResultAll, TResultPost, TPost> : IPostEndpoint<TResultPost, TPost>, IGetAllEndpoint<TResultAll>
{
}

public interface IPostGetAllEndpoint<TResult, TPost> : IPostGetAllEndpoint<TResult, TResult, TPost>
{
}
