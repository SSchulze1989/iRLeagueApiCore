namespace iRLeagueApiCore.Common.Models;
public class PostTriggerModel
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public TriggerType TriggerType { get; set; }
    /// <summary>
    /// Type of event that activates the trigger. Value is only valid for triggers of type Event.
    /// </summary>
    public TriggerEventType? EventType { get; set; }
    /// <summary>
    /// Time when the trigger will be activated. Value is only valid for triggers of type Time or Interval.
    /// Triggers of type Interval will re-set this value after each activation.
    /// </summary>
    public DateTimeOffset? Time { get; set; }
    /// <summary>
    /// Interval after which the trigger is re-activated. Value is only valid for triggers of type Interval.
    /// </summary>
    public TimeSpan? Interval { get; set; }
    /// <summary>
    /// Id used for reference in triggered events. Meaning of the id depends on the trigger event type
    /// </summary>
    public long? RefId1 { get; set; }
    /// <summary>
    /// Id used for reference in triggered events. Meaning of the id depends on the trigger event type
    /// </summary>
    public long? RefId2 { get; set; }
    public TriggerAction Action { get; set; }
    public Dictionary<string, object> ActionParameters { get; set; } = [];
}
