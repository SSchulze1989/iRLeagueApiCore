namespace iRLeagueApiCore.Common.Models;

public sealed class RawEventResultModel
{
    public long EventId { get; set; }
    public IEnumerable<RawSessionResultModel> SessionResults { get; set; } = Array.Empty<RawSessionResultModel>();
}
