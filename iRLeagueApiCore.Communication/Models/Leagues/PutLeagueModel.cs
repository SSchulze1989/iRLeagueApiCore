using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Schema for posting a new league
    /// </summary>
    [DataContract]
    public class PutLeagueModel
    {
        /// <summary>
        /// Full name of the league can contain any UTF-8 characters
        /// </summary>
        [DataMember]
        public string NameFull { get; set; }
    }
}
