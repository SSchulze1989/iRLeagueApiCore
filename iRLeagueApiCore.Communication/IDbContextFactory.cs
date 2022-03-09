using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication
{
    public interface IDbContextFactory
    {
        public T GetDbContext<T>();
    }
}
