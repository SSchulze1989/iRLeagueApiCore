using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Write a result to the database
    /// </summary>
    [DataContract]
    public class PutResultModel
    {
        /// <summary>
        /// Id of the session for which the result should be created/updated
        /// </summary>
        [DataMember(IsRequired = true)]
        public long SessionId { get; set; }
        /// <summary>
        /// [optional] Session details containing extended information about the iracing subsession 
        /// </summary>
        [DataMember]
        public SimSessionDetails SessionDetails { get; set; }
        /// <summary>
        /// List of entries 
        /// </summary>
        [DataMember(IsRequired = true)]
        public PutResultRowModel[] ResultRows { get; set; }
    }
}
