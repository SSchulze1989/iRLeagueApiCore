using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.QueryBuilder
{
    public class RouteBuilder : IRouteBuilder
    {
        private readonly List<string> parts;
        private readonly bool isRelative;
        
        private IParameterBuilder ParameterBuilder { get; set; }

        public RouteBuilder(bool isRelative = true)
        {
            parts = new List<string>();
            this.isRelative = isRelative;
        }

        public RouteBuilder(List<string> parts, bool isRelative = true)
        {
            this.parts = parts;
            this.isRelative = isRelative;
        }

        public IRouteBuilder AddEndpoint(string name)
        {
            parts.Add(name);
            return this;
        }

        public IRouteBuilder AddParameter<T>(T value)
        {
            parts.Add(value.ToString());
            return this;
        }

        public string Build()
        {
            var builder = new StringBuilder();
            builder.AppendJoin('/', parts);
            if (ParameterBuilder != null)
            {
                var parameterString = ParameterBuilder.Build();
                if (string.IsNullOrEmpty(parameterString) == false)
                {
                    builder.AppendFormat("?{0}", parameterString);
                }
            }
            return builder.ToString();
        }

        public IRouteBuilder WithParameters(Func<IParameterBuilder, IParameterBuilder> parameterBuilder)
        {
            ParameterBuilder = parameterBuilder?.Invoke(new ParameterBuilder());
            return this;
        }

        public RouteBuilder Copy()
        {
            return new RouteBuilder(parts, isRelative);
        }
    }
}
