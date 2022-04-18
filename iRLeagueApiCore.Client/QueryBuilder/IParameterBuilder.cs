using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Client.QueryBuilder
{
    public interface IParameterBuilder
    {
        public IParameterBuilder Add<T>(string name, T value);
        public IParameterBuilder AddArray<T>(string name, IEnumerable<T> values);
        public string Build();
    }
}
