using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Get a complete scored result from the database
    /// </summary>
    public class GetResultModel
    {
        /// <summary>
        /// Id of the league
        /// </summary>
        public long LeagueId { get; set; }
        /// <summary>
        /// Id of the season the result belongs to
        /// </summary>
        public long SeasonId { get; set; }
        /// <summary>
        /// Id of the session the result belongs to
        /// </summary>
        [DataMember]
        public long SessionId { get; set; }
        /// <summary>
        /// Session details containing extended information about the iracing subsession 
        /// </summary>
        [DataMember]
        public SimSessionDetails SessionDetails { get; set; }
        /// <summary>
        /// List of entries 
        /// </summary>
        [DataMember(IsRequired = true)]
        public GetResultRow[] ResultRows { get; set; }

    }
}
