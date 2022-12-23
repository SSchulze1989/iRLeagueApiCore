namespace iRLeagueApiCore.Services.ResultService.Models;

public class AcceptedReviewVoteCalculationData
{
    public long ReviewVoteId { get; set; }
    public long ReviewId { get; set; }
    public long? MemberAtFaultId { get; set; }
    public long? VoteCategoryId { get; set; }
    public int DefaultPenalty { get; set; }
}
