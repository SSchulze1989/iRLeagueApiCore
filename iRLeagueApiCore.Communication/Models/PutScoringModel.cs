using iRLeagueApiCore.Communication.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace iRLeagueApiCore.Communication.Models
{
    public class PutScoringModel
    {
        /// <summary>
        /// Id of the scoring - use 0 or omit for creating new scoring
        /// </summary>
        public long ScoringId { get; set; }
        /// <summary>
        /// Kind of scoring
        /// </summary>
        [EnumDataType(typeof(ScoringKindEnum))]
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public ScoringKindEnum ScoringKind { get; set; }
        /// <summary>
        /// Name of the scoring - shown in results
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Maximum number of results to use for calculating scorin group (e.g: Team scoring)
        /// </summary>
        public int MaxResultsPerGroup { get; set; }
        /// <summary>
        /// Take average of result in group. If false take sum
        /// </summary>
        public bool TakeGroupAverage { get; set; }
        /// <summary>
        /// External source for results if TakeResultsFromExtSource is true
        /// </summary>
        public long? ExtScoringSourceId { get; set; }
        /// <summary>
        /// Re-use calculated results from a different scoring (e.g: team scoring when individual results are already available)
        /// </summary>
        public bool TakeResultsFromExtSource { get; set; }
        /// <summary>
        /// Array of points awarded to each place - starting with [1st, 2nd, 3rd, ...]
        /// Points can have decimal values e.g.: 0.5
        /// </summary>
        public IEnumerable<double> BasePoints { get; set; }
        /// <summary>
        /// Array of key:value pairs as strings to award bonus points
        /// Syntax is always "key:points" for each entry
        /// Available keys:
        ///   p{nr} - finish position {nr} in race - example: winner gets 5 points,  = ["p1:5"]
        ///   q{nr} - position in qualifying - example: polesetter gets 1 point = ["q1:1"]
        ///   Both examples combined = ["p1:5","q1:1"]
        /// </summary>
        public IEnumerable<string> BonusPoints { get; set; }
        /// <summary>
        /// Id of the connected schedule when sessions are autofilled from schedule
        /// Not required when selecting sessions individually
        /// </summary>
        public long? ConnectedScheduleId { get; set; }
        /// <summary>
        /// Use teams information available from uploaded result set
        /// </summary>
        public bool UseResultSetTeam { get; set; }
        /// <summary>
        /// Update teams information on recalculation - this will overwrite the previous team in a scored row when a recalculation is triggered
        /// If you do not want the team to change after the result has been uploaded first (e.g.: team change during the runnin season) set to false
        /// </summary>
        public bool UpdateTeamOnRecalculation { get; set; }
        /// <summary>
        /// Options to use for sorting the result rows before issuing race and bonus points (should be enum actually)
        /// </summary>
        public string PointsSortOptions { get; set; }
        /// <summary>
        /// Options to use for sorting the result after points are calculated for the final order
        /// </summary>
        public string FinalSortOptions { get; set; }
        /// <summary>
        /// Show this result on the result page
        /// If false the results calculated from this scoring wont be 
        /// </summary>
        public bool ShowResults { get; set; }
        /// <summary>
        /// Long text description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// Array of session ids to select individual sessions for this scoring - will be overwritten if ConnectedScheduleId is set
        /// </summary>
        public IEnumerable<long> SessionIds { get; set; }

        //reserved for later
        //public int ScoringSessionType { get; set; }
        //public int SessionSelectType { get; set; }
        //public string ScoringWeightValues { get; set; }
        //public int AccumulateBy { get; set; }
        //public int AccumulateResultsOption { get; set; }
    }
}
