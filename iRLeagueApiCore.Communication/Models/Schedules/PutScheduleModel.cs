using System.Runtime.Serialization;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class PutScheduleModel
    {
        [DataMember]
        public string Name { get; set; }
    }
}
