using iRLeagueDatabaseCore.Models;

namespace iRLeagueApiCore.Services.ResultService.Models
{
    internal sealed class SessionCalculationData
    {
        public long LeagueId { get; set; }
        public long? SessionId { get; set; }
        public int? SessionNr { get; set; }
        public IEnumerable<AcceptedReviewVoteCalculationData> AcceptedReviewVotes { get; set; } = Array.Empty<AcceptedReviewVoteCalculationData>();
        public IEnumerable<ResultRowCalculationData> ResultRows { get; set; } = Array.Empty<ResultRowCalculationData>();
    }
}
