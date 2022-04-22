using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Responses
{
    public struct ValidationError
    {
        public string Property { get; set; }
        public string Error { get; set; }
        public object Value { get; set; }
    }
}
