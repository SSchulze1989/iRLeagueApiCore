using iRLeagueApiCore.Client.QueryBuilder;

namespace iRLeagueApiCore.Client.Endpoints;
public interface IEndpoint
{
    string QueryUrl { get; }
    internal void WithParameters(Func<IParameterBuilder, IParameterBuilder> parameterBuilder);
}

public static class IEndpointExtensions
{
    public static T AddQueryParameter<T>(this T endpoint, Func<IParameterBuilder, IParameterBuilder> parameterBuilder) where T : IEndpoint
    {
        endpoint.WithParameters(parameterBuilder);
        return endpoint;
    }
}
