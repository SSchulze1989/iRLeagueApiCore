using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class PutScheduleModel
    {
        [DataMember]
        public long ScheduleId { get; set; }
        [DataMember]
        public long SeasonId { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
