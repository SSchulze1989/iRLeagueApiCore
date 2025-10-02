using System.Diagnostics;

namespace iRLeagueApiCore.Common.Models;

[DataContract]
[DebuggerDisplay("{" + nameof(ToString) + "()}")]
public sealed class TriggerParameterModel
{
    [DataMember(EmitDefaultValue = false)]
    public bool OnlyOnce { get; set; } = false;
    [DataMember(EmitDefaultValue = false)]
    public TimeSpan? Interval { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public DateTimeOffset? Time { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public TriggerEventType? EventType { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public long? SeasonId { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public long? EventId { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public long? ReviewId { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public long? TeamId { get; set; }
    [DataMember(EmitDefaultValue = false)]
    public long? MemberId { get; set; }

    public override string ToString()
    {
        return $"OnlyOnce: {OnlyOnce}, Interval: {Interval}, Time: {Time}, SeasonId: {SeasonId}, EventId: {EventId}, ReviewId: {ReviewId}, TeamId: {TeamId}, MemberId: {MemberId}";
    }
}
