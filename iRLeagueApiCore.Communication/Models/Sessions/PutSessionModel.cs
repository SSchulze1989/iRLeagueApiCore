using System;
using System.Runtime.Serialization;
using iRLeagueApiCore.Communication.Enums;
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
        /// Short Title of the session
        /// </summary>
        [DataMember]
        public string SessionTitle { get; set; }
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
        //[DataMember]
        //public long RaceId { get; set; }
        /// <summary>
        /// Number of laps on the main event (0 for timed races)
        /// </summary>
        [DataMember]
        public int Laps { get; set; }
        /// <summary>
        /// [optional] Total length of the attached practice session - can be omitted if no practice attached
        /// </summary>
        [DataMember(IsRequired = false)]
        public TimeSpan? PracticeLength { get; set; }
        /// <summary>
        /// [optional] Total length of the attached qualy session - can be omitted if no qualy attached
        /// </summary>
        [DataMember(IsRequired = false)]
        public TimeSpan? QualyLength { get; set; }
        /// <summary>
        /// [optional] Total lenth of the attached race session - can be ommitted if not a race session type
        /// </summary>
        [DataMember]
        public TimeSpan? RaceLength { get; set; }
        // reserved for later
        //[DataMember]
        //public string IrSessionId { get; set; }
        //[DataMember]
        //public string IrResultLink { get; set; }
        /// <summary>
        /// Flag for attached qualy
        /// </summary>
        [DataMember]
        public bool QualyAttached { get; set; }
        /// <summary>
        /// Flag for attached practice
        /// </summary>
        [DataMember]
        public bool PracticeAttached { get; set; }
        /// <summary>
        /// Name of the session
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// If session is subsession provide the the numer here for ordering. If not leave at default (0)
        /// </summary>
        [DataMember(IsRequired = false)]
        public int SubSessionNr { get; set; }
    }
}
