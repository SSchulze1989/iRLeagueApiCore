using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Calculation;

namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class SessionResultCalculationConfiguration
    {
        public long LeagueId { get; set; }
        public long? SessionId { get; set; }
        public ScoringKind ScoringKind { get; set; }
        public string Name { get; set; } = string.Empty;
        public int MaxResultsPerGroup { get; set; }
        public bool UseResultSetTeam { get; set; }
        public bool UpdateTeamOnRecalculation { get; set; }

        public PointRule<ResultRowCalculationData> PointRule { get; set; } = PointRule<ResultRowCalculationData>.Default();
    }
}