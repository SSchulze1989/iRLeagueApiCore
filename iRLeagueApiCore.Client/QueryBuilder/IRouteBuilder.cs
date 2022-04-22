using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.QueryBuilder
{
    public interface IRouteBuilder
    {
        IRouteBuilder AddEndpoint(string name);
        IRouteBuilder AddParameter<T>(T value);
        IRouteBuilder WithParameters(Func<IParameterBuilder, IParameterBuilder> parameterBuilder);
        IRouteBuilder RemoveLast();
        string Build();
    }
}
