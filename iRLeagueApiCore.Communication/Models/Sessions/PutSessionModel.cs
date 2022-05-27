using System;
using System.Runtime.Serialization;
using iRLeagueApiCore.Communication.Enums;
using System.Collections.Generic;
#if NETCOREAPP
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#endif

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Scheme for creating and updating a session entry
    /// </summary>
    [DataContract]
    public class PutSessionModel
    {
        /// <summary>
        /// Name of the session
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Session type specifier - 0 = Undefined, 1 = Practice, 2 = Qualifying, 3 = Race, 4 = HeatEvent, 5 = Heat, 6 = Warmup
        /// </summary>
        [DataMember]
#if NETCOREAPP
        [EnumDataType(typeof(SessionType))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
#endif
        public SessionType SessionType { get; set; }
        /// <summary>
        /// Day and time of session start
        /// </summary>
        [DataMember]
        public DateTime? Date { get; set; }
        /// <summary>
        /// Track id of the location
        /// </summary>
        [DataMember]
        public long? TrackId { get; set; }
        /// <summary>
        /// Total duration of the session including all Subsessions and events
        /// </summary>
        [DataMember]
        public TimeSpan Duration { get; set; }
        [DataMember]
        public IEnumerable<PutSessionSubSessionModel> SubSessions { get; set; }
    }
}
