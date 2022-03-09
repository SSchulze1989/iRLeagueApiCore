using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class GetSeasonModel : PutSeasonModel
    {
        [DataMember]
        public DateTime? SeasonStart { get; set; }
        [DataMember]
        public DateTime? SeasonEnd { get; set; }
        [DataMember]
        public IEnumerable<long> ScheduleIds { get; set; }
    }
}
