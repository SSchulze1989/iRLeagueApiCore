﻿using System;
using System.Runtime.Serialization;

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Single row entry in a session result
    /// </summary>
    [DataContract]
    public class PutResultRowModel
    {
        /// <summary>
        /// Posiion at start of race session (equal to qually result when using attached qualifying)
        /// </summary>
        [DataMember]
        public double StartPosition { get; set; }
        /// <summary>
        /// Finish position in the race results (iracing penalties are applied - league penalties are not)
        /// </summary>
        [DataMember]
        public double FinishPosition { get; set; }
        /// <summary>
        /// Iracing id of the member
        /// </summary>
        [DataMember]
        public long MemberId { get; set; }
        /// <summary>
        /// Car number in the session
        /// </summary>
        [DataMember]
        public int CarNumber { get; set; }
        /// <summary>
        /// Class id in the session (in multiclass sessions)
        /// </summary>
        [DataMember]
        public int ClassId { get; set; }
        /// <summary>
        /// Name of the car (e.g: "Skip Barber RT200")
        /// </summary>
        [DataMember]
        public string Car { get; set; }
        /// <summary>
        /// Name of the car class (in multiclass sessions)
        /// </summary>
        [DataMember]
        public string CarClass { get; set; }
        /// <summary>
        /// Number of completed laps in the main session (only includes laps from one session type e.g: race)
        /// </summary>
        [DataMember]
        public double CompletedLaps { get; set; }
        /// <summary>
        /// Number of laps lead by this driver (only race)
        /// </summary>
        [DataMember]
        public double LeadLaps { get; set; }
        /// <summary>
        /// Number of the fastest laps of this driver (applicable to all session types)
        /// </summary>
        [DataMember]
        public int FastLapNr { get; set; }
        /// <summary>
        /// Number of incidents in the session (only main session)
        /// </summary>
        [DataMember]
        public double Incidents { get; set; }
        /// <summary>
        /// Driver status at the end of the race (checkered flag)
        /// </summary>
        [DataMember]
        public int Status { get; set; }
        /// <summary>
        /// Time set in qualifying (only available with attached qualy)
        /// </summary>
        [DataMember]
        public long QualifyingTime { get; set; }
        /// <summary>
        /// Interval to the leading driver 
        /// </summary>
        [DataMember]
        public TimeSpan Interval { get; set; }
        /// <summary>
        /// Average lap time in the main session
        /// </summary>
        [DataMember]
        public TimeSpan AvgLapTime { get; set; }
        /// <summary>
        /// Fastest lap time in the main session
        /// </summary>
        [DataMember]
        public TimeSpan FastestLapTime { get; set; }
        /// <summary>
        /// Position change StartPos -> FinPos during the main session
        /// </summary>
        [DataMember]
        public double PositionChange { get; set; }
        /// <summary>
        /// Irating before the event
        /// </summary>
        [DataMember]
        public int OldIrating { get; set; }
        /// <summary>
        /// Irating after completing the event
        /// </summary>
        [DataMember]
        public int NewIrating { get; set; }
        /// <summary>
        /// [optional] Irating at the start of the season
        /// When 0 or omitted the value is calculated based on the data in the database
        /// </summary>
        [DataMember(IsRequired = false)]
        public int SeasonStartIrating { get; set; }
        /// <summary>
        /// License class of the driver
        /// </summary>
        [DataMember]
        public string License { get; set; }
        /// <summary>
        /// Driver safety rating before the event
        /// </summary>
        [DataMember]
        public double OldSafetyRating { get; set; }
        /// <summary>
        /// Driver safety rating after completing the event
        /// </summary>
        [DataMember]
        public double NewSafetyRating { get; set; }
        /// <summary>
        /// Driver corners per incident before the event
        /// </summary>
        [DataMember]
        public int OldCpi { get; set; }
        /// <summary>
        /// Driver corners per incident after completing the event
        /// </summary>
        [DataMember]
        public int NewCpi { get; set; }
        /// <summary>
        /// Driver club id
        /// </summary>
        [DataMember]
        public int ClubId { get; set; }
        /// <summary>
        /// Driver club name
        /// </summary>
        [DataMember]
        public string ClubName { get; set; }
        /// <summary>
        /// Driver/Team car id
        /// </summary>
        [DataMember]
        public int CarId { get; set; }
        /// <summary>
        /// Completed race distance 
        /// When omited the value is calculated based on driver-laps/session-laps
        /// </summary>
        [DataMember]
        public double? CompletedPct { get; set; }
        /// <summary>
        /// Time at which the qualifying time was set (only with attached qualy)
        /// </summary>
        [DataMember]
        public DateTime? QualifyingTimeAt { get; set; }
        /// <summary>
        /// Iracing division of the driver
        /// </summary>
        [DataMember]
        public int Division { get; set; }
        /// <summary>
        /// Driver License level before the event
        /// </summary>
        [DataMember]
        public int OldLicenseLevel { get; set; }
        /// <summary>
        /// Driver license level after completing the event
        /// </summary>
        [DataMember]
        public int NewLicenseLevel { get; set; }
        /// <summary>
        /// [optional] Id of the team the driver was part in this event
        /// omit for no team
        /// </summary>
        [DataMember]
        public long? TeamId { get; set; }

        #region reserved_for_later
        ///// <summary>
        ///// Number of pitstops in the race
        ///// </summary>
        //[DataMember]
        //public int NumPitStops { get; set; }
        ///// <summary>
        ///// ???
        ///// </summary>
        //[DataMember]
        //public string PittedLaps { get; set; }
        //[DataMember]
        //public int NumOfftrackLaps { get; set; }
        //[DataMember]
        //public string OfftrackLaps { get; set; }
        //[DataMember]
        //public int NumContactLaps { get; set; }
        //[DataMember]
        //public string ContactLaps { get; set; }
        #endregion
    }
}