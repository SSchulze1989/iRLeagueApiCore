
using System.Text.Json;

namespace iRLeagueDatabaseCore.Models;

public partial class RosterEntryEntity
{
    public long LeagueId { get; set; }
    public long RosterId { get; set; }
    public long MemberId { get; set; }
    public long? TeamId { get; set; }
    public Dictionary<string, string> Profile { get; set; } = [];

    public virtual RosterEntity Roster { get; set; }
    public virtual LeagueMemberEntity Member { get; set; }
    public virtual TeamEntity Team { get; set; }
}

public class RosterEntryEntityConfiguration : IEntityTypeConfiguration<RosterEntryEntity>
{
    public void Configure(EntityTypeBuilder<RosterEntryEntity> entity)
    {
        entity.ToTable("RosterEntries");

        entity.HasKey(e => new { e.LeagueId, e.RosterId, e.MemberId });

        entity.HasIndex(e => new { e.RosterId, e.MemberId });

        entity.Property(e => e.Profile)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, default(JsonSerializerOptions)),
                new ValueComparer<Dictionary<string, string>>(false))
            .IsRequired(false);

        entity.HasOne(d => d.Roster)
            .WithMany(p => p.RosterEntries)
            .HasForeignKey(d => new { d.LeagueId, d.RosterId })
            .IsRequired(true);

        entity.HasOne(d => d.Member)
            .WithMany()
            .HasForeignKey(d => new { d.LeagueId, d.MemberId })
            .IsRequired(true);

        entity.HasOne(d => d.Team)
            .WithMany()
            .HasForeignKey(d => new { d.LeagueId, d.TeamId })
            .IsRequired(false);
    }
}
