using iRLeagueApiCore.Communication.Enums;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
#if NETCOREAPP  
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
#endif

namespace iRLeagueApiCore.Communication.Models
{
    /// <summary>
    /// Schema for creating a new scoring
    /// </summary>
    [DataContract]
    public class PostScoringModel
    {
        /// <summary>
        /// Kind of scoring
        /// </summary>
        [DataMember]
#if NETCOREAPP
        [EnumDataType(typeof(ScoringKind))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
#endif
        public ScoringKind ScoringKind { get; set; }
        /// <summary>
        /// Name of the scoring - shown in results
        /// </summary>
        [DataMember]
        public string Name { get; set; }
        /// <summary>
        /// Maximum number of results to use for calculating scorin group (e.g: Team scoring)
        /// </summary>
        [DataMember]
        public int MaxResultsPerGroup { get; set; }
        /// <summary>
        /// Take average of result in group. If false take sum
        /// </summary>
        [DataMember]
        public bool TakeGroupAverage { get; set; }
        /// <summary>
        /// External source for results if TakeResultsFromExtSource is true
        /// </summary>
        [DataMember]
        public long? ExtScoringSourceId { get; set; }
        /// <summary>
        /// Re-use calculated results from a different scoring (e.g: team scoring when individual results are already available)
        /// </summary>
        [DataMember]
        public bool TakeResultsFromExtSource { get; set; }
        /// <summary>
        /// Array of points awarded to each place - starting with [1st, 2nd, 3rd, ...]
        /// Points can have decimal values e.g.: 0.5
        /// </summary>
        [DataMember]
        public IEnumerable<double> BasePoints { get; set; }
        /// <summary>
        /// Array of key:value pairs as strings to award bonus points
        /// Syntax is always "key:points" for each entry
        /// Available keys:
        ///   p{nr} - finish position {nr} in race - example: winner gets 5 points,  = ["p1:5"]
        ///   q{nr} - position in qualifying - example: polesetter gets 1 point = ["q1:1"]
        ///   Both examples combined = ["p1:5","q1:1"]
        /// </summary>
        [DataMember]
        public IEnumerable<string> BonusPoints { get; set; }
        /// <summary>
        /// Id of the connected schedule when sessions are autofilled from schedule
        /// Not required when selecting sessions individually
        /// </summary>
        [DataMember(IsRequired = false)]
        public long? ConnectedScheduleId { get; set; }
        /// <summary>
        /// Use teams information available from uploaded result set
        /// </summary>
        [DataMember]
        public bool UseResultSetTeam { get; set; }
        /// <summary>
        /// Update teams information on recalculation - this will overwrite the previous team in a scored row when a recalculation is triggered
        /// If you do not want the team to change after the result has been uploaded first (e.g.: team change during the runnin season) set to false
        /// </summary>
        [DataMember]
        public bool UpdateTeamOnRecalculation { get; set; }
        /// <summary>
        /// Options to use for sorting the result rows before issuing race and bonus points (should be enum actually)
        /// </summary>
        [DataMember]
        public string PointsSortOptions { get; set; }
        /// <summary>
        /// Options to use for sorting the result after points are calculated for the final order
        /// </summary>
        [DataMember]
        public string FinalSortOptions { get; set; }
        /// <summary>
        /// Show this result on the result page
        /// If false the results calculated from this scoring wont be 
        /// </summary>
        [DataMember]
        public bool ShowResults { get; set; }
        /// <summary>
        /// Long text description
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Type of session that this scoring can be applied to
        /// </summary>
        [DataMember]
        public SessionType ScoringSessionType { get; set; }
        /// <summary>
        /// Defines how the sessions for this scoring are selected
        /// </summary>
        [DataMember]
        public ScoringSessionSelectionType SessionSelectType { get; set; }
        /// <summary>
        /// List of weights for each accumulated scoring separated by ',' - only for accumlated scorings
        /// </summary>
        [DataMember]
        public string ScoringWeightValues { get; set; }
        /// <summary>
        /// Select the column for identifying rows that belong together
        /// </summary>
        [DataMember]
        public AccumulateByOption AccumulateBy { get; set; }
        /// <summary>
        /// Select the method to accumulate scorings
        /// </summary>
        [DataMember]
        public AccumulateResultsOption AccumulateResultsOption { get; set; }
        /// <summary>
        /// Number of average races when using <see cref="TakeGroupAverage"/>
        /// </summary>
        [DataMember]
        public int AverageRaceNr { get; set; }
    }
}
