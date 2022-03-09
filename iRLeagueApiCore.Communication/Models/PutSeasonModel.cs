using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class PutSeasonModel : VersionModel
    {
        [DataMember]
        public long SeasonId { get; set;}
        [DataMember]
        public long LeagueId { get; set;}
        [DataMember]
        public string SeasonName { get; set;}
        [DataMember]
        public long? MainScoringId { get; set; }
        [DataMember]
        public bool HideComments { get; set; }
        [DataMember]
        public bool Finished { get; set; }
    }
}
