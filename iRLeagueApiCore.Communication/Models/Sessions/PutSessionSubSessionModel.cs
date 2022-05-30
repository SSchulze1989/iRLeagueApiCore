using iRLeagueApiCore.Communication.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
#if NETCOREAPP
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#endif

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Scheme for subsession data when putting to a session entity
    /// </summary>
    [DataContract]
    public class PutSessionSubSessionModel
    {
        /// <summary>
        /// Identifier
        /// </summary>
        [DataMember]
        public long SubSessionId { get; set; }
        /// <summary>
        /// Number that decides order of subsessions
        /// </summary>
        [DataMember]
        public int SubSessionNr { get; set; }
        /// <summary>
        /// Optional name
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Type of the subsession
        /// </summary>
        [DataMember]
#if NETCOREAPP
        [EnumDataType(typeof(SimSessionType))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
#endif
        public SimSessionType SessionType { get; set; }
        /// <summary>
        /// Offset start time from the start of the parent session
        /// </summary>
        [DataMember]
        public TimeSpan StartOffset { get; set; }
        /// <summary>
        /// Duration (max.) of this subsession
        /// </summary>
        [DataMember]
        public TimeSpan Duration { get; set; }
        /// <summary>
        /// Number of laps (max.) in this subsession
        /// </summary>
        [DataMember]
        public int Laps { get; set; }
    }
}
