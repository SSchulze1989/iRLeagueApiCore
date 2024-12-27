using iRLeagueApiCore.Common.Models;

namespace iRLeagueApiCore.Services.ResultService.Models;

public sealed class ReviewPenaltyCalculationResult
{
    public long ReviewId { get; set; }
    public long? ReviewVoteId { get; set; }
    public PenaltyModel Value { get; set; } = new();
}
