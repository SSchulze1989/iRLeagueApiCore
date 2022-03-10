using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class PutLeagueModel
    {
        [DataMember]
        public long LeagueId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string NameFull { get; set; }
    }
}
