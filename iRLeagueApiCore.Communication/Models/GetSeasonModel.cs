using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class GetSeasonModel : PutSeasonModel, IVersionModel
    {
        [DataMember]
        public long LeagueId { get; set; }
        [DataMember]
        public DateTime? SeasonStart { get; set; }
        [DataMember]
        public DateTime? SeasonEnd { get; set; }
        [DataMember]
        public IEnumerable<long> ScheduleIds { get; set; }

        #region version
        [DataMember]
        public DateTime? CreatedOn { get; set; } = null;
        [DataMember]
        public DateTime? LastModifiedOn { get; set; } = null;
        [DataMember]
        public string CreatedByUserId { get; set; }
        [DataMember]
        public string LastModifiedByUserId { get; set; }
        [DataMember]
        public string CreatedByUserName { get; set; }
        [DataMember]
        public string LastModifiedByUserName { get; set; }
        #endregion
    }
}
