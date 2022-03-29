using System.Runtime.Serialization;

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
