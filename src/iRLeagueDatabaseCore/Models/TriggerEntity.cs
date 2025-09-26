using iRLeagueApiCore.Common.Models;

namespace iRLeagueDatabaseCore.Models;
public partial class TriggerEntity : Revision, IVersionEntity
{
    public long LeagueId { get; set; }
    public long TriggerId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TriggerType TriggerType { get; set; }
    public TriggerParameterModel Parameters { get; set; }
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

        entity.Property(e => e.Parameters)
            .HasConversion(
                v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions)null),
                v => System.Text.Json.JsonSerializer.Deserialize<TriggerParameterModel>(v, (System.Text.Json.JsonSerializerOptions)null),
                new ValueComparer<TriggerParameterModel>(false));

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
