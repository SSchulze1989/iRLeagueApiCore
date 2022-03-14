using iRLeagueApiCore.Communication.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Scheme for fetching a session entry
    /// </summary>
    [DataContract]
    public class GetSessionModel : PutSessionModel, IVersionModel
    {
        /// <summary>
        /// Unique identifier
        /// </summary>
        [DataMember]
        public new long SessionId { get; set; }
        /// <summary>
        /// Id of the league this session belongs to
        /// </summary>
        [DataMember]
        public long LeagueId { get; set; }

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
