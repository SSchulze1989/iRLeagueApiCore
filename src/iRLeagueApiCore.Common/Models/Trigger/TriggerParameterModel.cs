using System.Diagnostics;

namespace iRLeagueApiCore.Common.Models;

[DataContract]
[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public sealed class TriggerParameterModel
{
    [DataMember(EmitDefaultValue = false)]
    public TimeSpan? Interval { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public DateTimeOffset? Time { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public TriggerEventType? EventType { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public long? RefId1 { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public long? RefId2 { get; set; }

    public override string ToString()
    {
        return $"Interval: {Interval}, Time: {Time}, RefId1: {RefId1}, RefId2: {RefId2}";
    }
}
