using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Schema for fetching an existing scoring
    /// </summary>
    public class GetScoringModel : PutScoringModel, IVersionModel
    {
        /// <summary>
        /// Id of the scoring
        /// </summary>
        [DataMember]
        public long Id { get; set; }
        /// <summary>
        /// Id of the league the scoring belongs to
        /// </summary>
        [DataMember]
        public long LeagueId { get; set; }
        /// <summary>
        /// Id of the season the scoring belongs to
        /// </summary>
        [DataMember]
        public long SeasonId { get; set; }
        /// <summary>
        /// Ids of session connected to the scoring
        /// </summary>
        [DataMember]
        public IEnumerable<long> SessionIds { get; set; }
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
