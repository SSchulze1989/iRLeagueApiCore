using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Scheme for fetching a session entry
    /// </summary>
    [DataContract]
    public class SessionModel : PutSessionModel, IVersionModel
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [DataMember]
        public long SessionId { get; set; }
        /// <summary>
        /// Id of the league this session belongs to
        /// </summary>
        [DataMember]
        public long LeagueId { get; set; }
        /// <summary>
        /// Id of the schedule this session belongs to
        /// </summary>
        [DataMember]
        public long? ScheduleId { get; set; }
        /// <summary>
        /// If session is subsession provide the id of the parent session here. If not leave at default (null)
        /// </summary>
        [DataMember(IsRequired = false)]
        public long? ParentSessionId { get; set; }
        /// <summary>
        /// Flag shows if result is available
        /// </summary>
        [DataMember]
        public bool HasResult { get; set; }
        /// <summary>
        /// List of subsessions
        /// </summary>
        [DataMember]
        public new IEnumerable<SubSessionModel> SubSessions { get; set; }

        #region version
        /// <summary>
        /// Date of creation
        /// </summary>
        [DataMember]
        public DateTime? CreatedOn { get; set; }
        /// <summary>
        /// Date of last modification
        /// </summary>
        [DataMember]
        public DateTime? LastModifiedOn { get; set; }
        /// <summary>
        /// User id that created the entry
        /// </summary>
        [DataMember]
        public string CreatedByUserId { get; set; }
        /// <summary>
        /// User id that last modified the entry
        /// </summary>
        [DataMember]
        public string LastModifiedByUserId { get; set; }
        /// <summary>
        /// User name that created the entry
        /// </summary>
        [DataMember]
        public string CreatedByUserName { get; set; }
        /// <summary>
        /// User name that last modified the entry
        /// </summary>
        [DataMember]
        public string LastModifiedByUserName { get; set; }
        #endregion
    }
}
