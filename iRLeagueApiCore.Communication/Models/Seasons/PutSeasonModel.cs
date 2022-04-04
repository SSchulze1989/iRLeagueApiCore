using System.Runtime.Serialization;

namespace iRLeagueApiCore.Communication.Models
{
    [DataContract]
    public class PutSeasonModel
    {
        [DataMember]
        public string SeasonName { get; set; }
        [DataMember]
        public long? MainScoringId { get; set; }
        [DataMember]
        public bool HideComments { get; set; }
        [DataMember]
        public bool Finished { get; set; }
    }
}
