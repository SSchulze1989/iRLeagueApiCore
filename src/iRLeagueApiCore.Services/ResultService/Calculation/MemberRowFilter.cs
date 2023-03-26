using iRLeagueApiCore.Common.Enums;
using iRLeagueApiCore.Services.ResultService.Models;

namespace iRLeagueApiCore.Services.ResultService.Calculation;
internal sealed class MemberRowFilter : RowFilter<ResultRowCalculationResult>
{
    public IReadOnlyCollection<long> MemberIds;
    public MatchedValueAction Action;

    /// <summary>
    /// Create new instance of member row filter
    /// </summary>
    /// <param name="memberIdValues"></param>
    /// <param name="action"></param>
    /// <exception cref="ArgumentException">If member id cannot be parsed as long</exception>
    public MemberRowFilter(IEnumerable<string> memberIdValues, MatchedValueAction action)
    {
        MemberIds = memberIdValues.Select(GetMemberId).ToList();
        this.Action = action;
    }

    public override IEnumerable<T> FilterRows<T>(IEnumerable<T> rows)
    {
        var match = rows.Where(x => MemberIds.Contains(x.MemberId.GetValueOrDefault()));
        return Action switch
        {
            MatchedValueAction.Keep => match,
            MatchedValueAction.Remove => rows.Except(match),
            _ => rows,
        };
    }

    private long GetMemberId(string idString)
    {
        if (long.TryParse(idString, out var memberId)) 
        {  
            return memberId; 
        }
        throw new ArgumentException($"Argument \"{idString}\" is not a valid member id");
    }
}
