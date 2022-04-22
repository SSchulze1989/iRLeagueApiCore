using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.QueryBuilder
{
    public interface IRouteBuilder
    {
        public IRouteBuilder AddEndpoint(string name);
        public IRouteBuilder AddParameter<T>(T value);
        public IRouteBuilder WithParameters(Func<IParameterBuilder, IParameterBuilder> parameterBuilder);
        public IRouteBuilder RemoveLast();
        public string Build();
    }
}
