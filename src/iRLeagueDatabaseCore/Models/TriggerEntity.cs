using iRLeagueApiCore.Common.Models;
using System.Runtime.Serialization;

namespace iRLeagueDatabaseCore.Models;
public partial class TriggerEntity : Revision, IVersionEntity
{
    public long LeagueId { get; set; }
    public long TriggerId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TriggerType TriggerType { get; set; }
    /// <summary>
    /// Type of event that activates the trigger. Value is only valid for triggers of type Event.
    /// </summary>
    public TriggerEventType? EventType { get; set; }
    /// <summary>
    /// Time when the trigger will be activated. Value is only valid for triggers of type Time or Interval.
    /// Triggers of type Interval will re-set this value after each activation.
    /// </summary>
    public DateTimeOffset? TimeElapses { get; set; }
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
    public Dictionary<string, object> ActionParameters { get; set; }

    public virtual LeagueEntity League { get; set; }
}

public class TriggerEntityConfiguration : IEntityTypeConfiguration<TriggerEntity>
{
    public void Configure(EntityTypeBuilder<TriggerEntity> entity)
    {
        entity.ToTable("Triggers");

        entity.HasKey(e => new { e.LeagueId, e.TriggerId });

        entity.HasAlternateKey(e => e.TriggerId);

        entity.Property(e => e.TriggerId)
            .ValueGeneratedOnAdd();

        entity.Property(e => e.TriggerType)
            .HasConversion<string>();

        entity.HasIndex(e => e.TriggerType);

        entity.Property(e => e.Interval)
            .HasConversion<TimeSpanToTicksConverter>();

        entity.HasIndex(e => e.RefId1);

        entity.HasIndex(e => e.RefId2);

        entity.Property(e => e.Action)
            .HasConversion<string>();

        entity.Property(e => e.ActionParameters)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(v, (System.Text.Json.JsonSerializerOptions)null),
                new ValueComparer<Dictionary<string, object>>(false));

        entity.HasOne(d => d.League)
            .WithMany(p => p.Triggers)
            .HasForeignKey(d => d.LeagueId);
    }
}
