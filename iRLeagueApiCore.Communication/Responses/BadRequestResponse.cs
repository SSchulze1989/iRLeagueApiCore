using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Responses
{
    public struct BadRequestResponse
    {
        public string Status { get; set; }
        public IEnumerable<ValidationError> Errors { get; set; }
    }
}
