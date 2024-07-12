namespace iRLeagueApiCore.Common.Models;

public sealed class RawSessionResultModel
{
    public long SessionId { get; set; }
    public IEnumerable<RawResultRowModel> ResultRows { get; set; } = Array.Empty<RawResultRowModel>();
}
