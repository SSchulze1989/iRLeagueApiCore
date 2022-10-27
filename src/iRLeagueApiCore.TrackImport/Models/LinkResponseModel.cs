using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.TrackImport.Models
{
    public struct LinkResponseModel
    {
        public string link { get; set; }
        public DateTime expires { get; set; }
    }
}
