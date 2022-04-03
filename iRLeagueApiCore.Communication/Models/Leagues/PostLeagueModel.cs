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
    public class PostLeagueModel
    {
        /// <summary>
        /// Short name of the league
        /// <para/>Used to identify the league in queries
        /// <para/>Cannot contain spaces and only use characters: a-z A-Z 0-1 _ -
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Full name of the league can contain any UTF-8 characters
        /// </summary>
        [DataMember]
        public string NameFull { get; set; }
    }
}
