#nullable disable

using iRLeagueApiCore.Common.Models;
using System.Text.Json;

namespace iRLeagueDatabaseCore.Models;

public partial class LeagueMemberEntity
{
    public LeagueMemberEntity()
    {
        ProtestsInvolved = new HashSet<ProtestEntity>();
    }

    public long MemberId { get; set; }
    public long LeagueId { get; set; }
    public long? TeamId { get; set; }
    public string Number { get; set; }
    public string DiscordId { get; set; }
    public string CountryFlag { get; set; }
    public Dictionary<string, string> Profile { get; set; } = [];

    public virtual MemberEntity Member { get; set; }
    public virtual LeagueEntity League { get; set; }
    public virtual TeamEntity Team { get; set; }
    public virtual IEnumerable<ProtestEntity> ProtestsInvolved { get; set; }
}

public class LeagueMemberEntityConfiguration : IEntityTypeConfiguration<LeagueMemberEntity>
{
    public void Configure(EntityTypeBuilder<LeagueMemberEntity> entity)
    {
        entity.HasKey(e => new { e.LeagueId, e.MemberId });

        entity.HasIndex(e => e.MemberId);

        entity.Property(e => e.Profile)
            .HasColumnType("json")
            .HasConversion(
                v => JsonSerializer.Serialize(v, default(JsonSerializerOptions)),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, default(JsonSerializerOptions)),
                new ValueComparer<Dictionary<string, string>>(false))
            .IsRequired(false);

        entity.Property(e => e.Number)
            .HasMaxLength(3);

        entity.Property(e => e.DiscordId)
            .HasMaxLength(255);

        entity.Property(e => e.CountryFlag)
            .HasMaxLength(3);

        entity.HasOne(e => e.League)
            .WithMany(e => e.LeagueMembers)
            .HasForeignKey(e => e.LeagueId);

        entity.HasOne(e => e.Team)
            .WithMany(e => e.Members)
            .HasForeignKey(e => new { e.LeagueId, e.TeamId });
    }
}
