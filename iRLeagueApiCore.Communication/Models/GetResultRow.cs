using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Get a scored result row from the database
    /// </summary>
    public class GetResultRow : PutResultRowModel
    {
        /// <summary>
        /// First name of the driver
        /// </summary>
        public string Firstname { get; set; }
        /// <summary>
        /// Last name of the driver
        /// </summary>
        public string Lastname { get; set; }
        /// <summary>
        /// Team name of the drivers team (or team result)
        /// </summary>
        public string TeamName { get; set; }
        /// <summary>
        /// Points gained from result in the race
        /// </summary>
        [DataMember]
        public double RacePoints { get; set; }
        /// <summary>
        /// Points gained from bonus condition (will be added to race points)
        /// </summary>
        [DataMember]
        public double BonusPoints { get; set; }
        /// <summary>
        /// Points deducted as penalty (Value is positive but points will be deducted from race points)
        /// </summary>
        [DataMember]
        public double PenaltyPoints { get; set; }
        /// <summary>
        /// Total scored points -> sum of: (RacePoints + BonusPoints - PenaltyPoints)
        /// </summary>
        [DataMember]
        public double TotalPoints { get; set; }
        /// <summary>
        /// Final position after all scoring rules and penalties are applied
        /// </summary>
        [DataMember]
        public int FinalPosition { get; set; }
        /// <summary>
        /// Position change StartPosition -> FinalPosition
        /// </summary>
        [DataMember]
        public double FinalPositionChange { get; set; }
    }
}
