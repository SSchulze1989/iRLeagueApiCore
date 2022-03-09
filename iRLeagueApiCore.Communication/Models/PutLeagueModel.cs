using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class PutLeagueModel : VersionModel
    {
        [DataMember(IsRequired = true)]
        public int LeagueId { get; set; }
        [DataMember]
        public string Name { get; set; }
        [DataMember]
        public string NameFull { get; set; }
    }
}
