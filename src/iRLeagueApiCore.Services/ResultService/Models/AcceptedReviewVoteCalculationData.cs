using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Services.ResultService.Models;

public sealed class AcceptedReviewVoteCalculationData
{
    public long ReviewVoteId { get; set; }
    public long ReviewId { get; set; }
    public long? MemberAtFaultId { get; set; }
    public long? TeamAtFaultId { get; set; }
    public long? VoteCategoryId { get; set; }
    public PenaltyModel DefaultPenalty { get; set; } = new();
}
