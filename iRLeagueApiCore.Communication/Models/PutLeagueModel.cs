using System.Runtime.Serialization;

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
